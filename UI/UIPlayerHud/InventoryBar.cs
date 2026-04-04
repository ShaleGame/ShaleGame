using Godot;
using System.Linq;
using CrossedDimensions.Components;
using CrossedDimensions.Items;

namespace CrossedDimensions.UI;

public partial class InventoryBar : Control
{
	private static readonly PackedScene InventorySlotScene = GD.Load<PackedScene>("res://UI/UIPlayerHud/InventorySlot.tscn");

	private Godot.Collections.Array<InventorySlot> _slots = new();
	private InventoryComponent _inventory;
	private HBoxContainer _hboxContainer;

	public override void _Ready()
	{
		_hboxContainer = GetNode<HBoxContainer>("%HBoxContainer");
	}

	public void SetInventory(InventoryComponent inventory)
	{
		if (_inventory != null)
		{
			_inventory.EquippedWeaponChanged -= OnEquippedWeaponChanged;
			_inventory.WeaponAdded -= OnWeaponAdded;
			_inventory.WeaponRemoved -= OnWeaponRemoved;
		}

		_inventory = inventory;

		if (_inventory == null)
        {
			return;
        }

		_inventory.EquippedWeaponChanged += OnEquippedWeaponChanged;
		_inventory.WeaponAdded += OnWeaponAdded;
		_inventory.WeaponRemoved += OnWeaponRemoved;

		RefreshAllSlots();
	}

	private void OnEquippedWeaponChanged(Weapon previousWeapon, int previousIndex, Weapon currentWeapon, int currentIndex)
	{
		UpdateSelection();
	}

	private void OnWeaponAdded(Weapon weapon, int index)
	{
		RefreshAllSlots();
	}

	private void OnWeaponRemoved(Weapon weapon, int index)
	{
		RefreshAllSlots();
	}

	private void RefreshAllSlots()
	{
		if (!IsInstanceValid(this))
		{
			// check to see if this node has been freed; this code
			// could still run
			return;
		}

		foreach (var slot in _slots)
		{
			slot.QueueFree();
		}
		_slots.Clear();

		if (_inventory == null)
			return;

		var weapons = _inventory.GetChildren().Where(c => c is Weapon).Cast<Weapon>().ToList();
		var equipped = _inventory.EquippedWeapon;

		foreach (var weapon in weapons)
		{
			var slot = InventorySlotScene.Instantiate<InventorySlot>();
			_hboxContainer.AddChild(slot);
			slot.SetWeapon(weapon);
			slot.SetSelected(weapon == equipped);
			_slots.Add(slot);
		}
	}

	private void UpdateSelection()
	{
		if (_inventory == null)
			return;

		var weapons = _inventory.GetChildren().Where(c => c is Weapon).Cast<Weapon>().ToList();
		for (int i = 0; i < _slots.Count; i++)
		{
			if (i < weapons.Count)
			{
				_slots[i].SetSelected(weapons[i] == _inventory.EquippedWeapon);
			}
		}
	}
}
