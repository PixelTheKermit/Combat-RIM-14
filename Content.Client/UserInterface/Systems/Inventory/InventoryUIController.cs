﻿using Content.Client.Gameplay;
using Content.Client.Inventory;
using Content.Client.Storage;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Inventory.Controls;
using Content.Client.UserInterface.Systems.Inventory.Windows;
using Content.Shared.Input;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Utility;
using static Content.Client.Inventory.ClientInventorySystem;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.UserInterface.Systems.Inventory;

public sealed class InventoryUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>,
    IOnSystemChanged<ClientInventorySystem>
{
    [Dependency] private readonly IEntityManager _entities = default!;

    [UISystemDependency] private readonly ClientInventorySystem _inventorySystem = default!;

    private ClientInventoryComponent? _playerInventory;
    private readonly Dictionary<string, ItemSlotButtonContainer> _slotGroups = new();

    private StrippingWindow? _strippingWindow;
    private ItemSlotButtonContainer? _inventoryHotbar;
    private MenuButton? _inventoryButton;

    public void OnStateEntered(GameplayState state)
    {
        DebugTools.Assert(_strippingWindow == null);
        _strippingWindow = UIManager.CreateWindow<StrippingWindow>();
        _inventoryButton = UIManager.GetActiveUIWidget<MenuBar.Widgets.GameTopMenuBar>().InventoryButton;
        LayoutContainer.SetAnchorPreset(_strippingWindow, LayoutContainer.LayoutPreset.Center);

        //bind open inventory key to OpenInventoryMenu;
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OpenInventoryMenu, InputCmdHandler.FromDelegate(_ => ToggleInventoryBar()))
            .Register<ClientInventorySystem>();
        _inventoryButton.OnPressed += InventoryButtonPressed;
    }

    public void OnStateExited(GameplayState state)
    {
        if (_strippingWindow != null)
        {
            _strippingWindow.Dispose();
            _strippingWindow = null;
        }

        if (_inventoryHotbar != null)
        {
            _inventoryHotbar.Visible = false;
        }

        if (_inventoryButton != null)
        {
            _inventoryButton.OnPressed -= InventoryButtonPressed;
            _inventoryButton.Pressed = false;
            _inventoryButton = null;
        }

        CommandBinds.Unregister<ClientInventorySystem>();
    }

    public void RegisterInventoryBarContainer(ItemSlotButtonContainer inventoryHotbar)
    {
        _inventoryHotbar = inventoryHotbar;
    }

    private void InventoryButtonPressed(ButtonEventArgs args)
    {
        ToggleInventoryBar();
    }

    private void UpdateInventoryHotbar(ClientInventoryComponent? clientInv)
    {
        if (clientInv == null)
        {
            _inventoryHotbar?.ClearButtons();
            return;
        }

        foreach (var (_, data) in clientInv.SlotData)
        {
            if (!data.ShowInWindow || !_slotGroups.TryGetValue(data.SlotGroup, out var container))
                continue;

            if (!container.TryGetButton(data.SlotName, out var button))
            {
                button = new SlotButton(data);
                button.Pressed += ItemPressed;
                button.StoragePressed += StoragePressed;
                container.AddButton(button);
            }

            var sprite = _entities.GetComponentOrNull<SpriteComponent>(data.HeldEntity);
            var showStorage = _entities.HasComponent<ClientStorageComponent>(data.HeldEntity);
            var update = new SlotSpriteUpdate(data.SlotGroup, data.SlotName, sprite, showStorage);
            SpriteUpdated(update);
        }
    }

    private void UpdateStrippingWindow(ClientInventoryComponent? clientInv)
    {
        if (clientInv == null)
        {
            _strippingWindow!.InventoryButtons.ClearButtons();
            return;
        }

        foreach (var (_, data) in clientInv.SlotData)
        {
            if (!data.ShowInWindow)
                continue;

            if (!_strippingWindow!.InventoryButtons.TryGetButton(data.SlotName, out var button))
            {
                button = new SlotButton(data);
                button.Pressed += ItemPressed;
                button.StoragePressed += StoragePressed;
                _strippingWindow!.InventoryButtons.AddButton(button, data.ButtonOffset);
            }

            var sprite = _entities.GetComponentOrNull<SpriteComponent>(data.HeldEntity);
            var showStorage = _entities.HasComponent<ClientStorageComponent>(data.HeldEntity);
            var update = new SlotSpriteUpdate(data.SlotGroup, data.SlotName, sprite, showStorage);
            SpriteUpdated(update);
        }
    }

    public void ToggleStrippingMenu()
    {
        UpdateStrippingWindow(_playerInventory);
        if (_strippingWindow!.IsOpen)
        {
            _strippingWindow!.Close();
            return;
        }

        _strippingWindow.Open();
    }

    public void ToggleInventoryBar()
    {
        if (_inventoryHotbar == null)
        {
            Logger.Warning("Tried to toggle inventory bar when none are assigned");
            return;
        }

        UpdateInventoryHotbar(_playerInventory);
        if (_inventoryHotbar.Visible)
        {
            _inventoryHotbar.Visible = false;
            if (_inventoryButton != null)
                _inventoryButton.Pressed = false;
        }
        else
        {
            _inventoryHotbar.Visible = true;
            if (_inventoryButton != null)
                _inventoryButton.Pressed = true;
        }
    }

    // Neuron Activation
    public void OnSystemLoaded(ClientInventorySystem system)
    {
        _inventorySystem.OnSlotAdded += AddSlot;
        _inventorySystem.OnSlotRemoved += RemoveSlot;
        _inventorySystem.OnLinkInventory += LoadSlots;
        _inventorySystem.OnUnlinkInventory += UnloadSlots;
        _inventorySystem.OnSpriteUpdate += SpriteUpdated;
    }

    // Neuron Deactivation
    public void OnSystemUnloaded(ClientInventorySystem system)
    {
        _inventorySystem.OnSlotAdded -= AddSlot;
        _inventorySystem.OnSlotRemoved -= RemoveSlot;
        _inventorySystem.OnLinkInventory -= LoadSlots;
        _inventorySystem.OnUnlinkInventory -= UnloadSlots;
        _inventorySystem.OnSpriteUpdate -= SpriteUpdated;
    }

    private void ItemPressed(GUIBoundKeyEventArgs args, SlotControl control)
    {
        var slot = control.SlotName;

        if (args.Function == EngineKeyFunctions.UIClick)
        {
            _inventorySystem.UIInventoryActivate(control.SlotName);
            return;
        }

        if (args.Function == ContentKeyFunctions.ActivateItemInWorld)
        {
            _inventorySystem.UIInventoryStorageActivate(control.SlotName);
            return;
        }

        if (_playerInventory == null)
        {
            return;
        }

        if (args.Function == ContentKeyFunctions.ExamineEntity)
        {
            _inventorySystem.UIInventoryExamine(slot, _playerInventory.Owner);
        }
        else if (args.Function == EngineKeyFunctions.UseSecondary)
        {
            _inventorySystem.UIInventoryOpenContextMenu(slot, _playerInventory.Owner);
        }
        else if (args.Function == ContentKeyFunctions.ActivateItemInWorld)
        {
            _inventorySystem.UIInventoryActivateItem(slot, _playerInventory.Owner);
        }
        else if (args.Function == ContentKeyFunctions.AltActivateItemInWorld)
        {
            _inventorySystem.UIInventoryAltActivateItem(slot, _playerInventory.Owner);
        }
    }

    private void StoragePressed(GUIBoundKeyEventArgs args, SlotControl control)
    {
        _inventorySystem.UIInventoryStorageActivate(control.SlotName);
    }

    private void AddSlot(SlotData data)
    {
        if (!_slotGroups.TryGetValue(data.SlotGroup, out var slotGroup))
            return;

        var button = new SlotButton(data);
        button.Pressed += ItemPressed;
        button.StoragePressed += StoragePressed;
        slotGroup.AddButton(button);
    }

    private void RemoveSlot(SlotData data)
    {
        if (!_slotGroups.TryGetValue(data.SlotGroup, out var slotGroup))
            return;

        slotGroup.RemoveButton(data.SlotName);
    }

    private void LoadSlots(ClientInventoryComponent clientInv)
    {
        UnloadSlots();
        _playerInventory = clientInv;
        foreach (var slotData in clientInv.SlotData.Values)
        {
            AddSlot(slotData);
        }

        UpdateInventoryHotbar(_playerInventory);
    }

    private void UnloadSlots()
    {
        _playerInventory = null;
        foreach (var slotGroup in _slotGroups.Values)
        {
            slotGroup.ClearButtons();
        }
    }

    private void SpriteUpdated(SlotSpriteUpdate update)
    {
        var (group, name, sprite, showStorage) = update;

        if (_strippingWindow?.InventoryButtons.GetButton(update.Name) is { } inventoryButton)
        {
            inventoryButton.SpriteView.Sprite = sprite;
            inventoryButton.StorageButton.Visible = showStorage;
        }

        if (_slotGroups.GetValueOrDefault(group)?.GetButton(name) is not { } button)
            return;

        button.SpriteView.Sprite = sprite;
        button.StorageButton.Visible = showStorage;
    }

    public bool RegisterSlotGroupContainer(ItemSlotButtonContainer slotContainer)
    {
        if (_slotGroups.TryAdd(slotContainer.SlotGroup, slotContainer))
            return true;

        Logger.Warning("Could not add container for slotgroup: " + slotContainer.SlotGroup);
        return false;
    }

    public void RemoveSlotGroup(string slotGroupName)
    {
        _slotGroups.Remove(slotGroupName);
    }
}
