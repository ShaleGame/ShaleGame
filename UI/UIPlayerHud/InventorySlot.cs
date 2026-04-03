using Godot;
using CrossedDimensions.Items;

namespace CrossedDimensions.UI;

public partial class InventorySlot : Control
{
	private Weapon _weapon;
	private bool _isSelected;

	[Export]
	public Weapon Weapon
	{
		get => _weapon;
		set
		{
			_weapon = value;
			UpdateDisplay();
		}
	}

	private TextureRect _icon;
	private PanelContainer _border;
	private Label _placeholderIcon;

	public override void _Ready()
	{
		_icon = GetNode<TextureRect>("%Icon");
		_border = GetNode<PanelContainer>("%Border");
		_placeholderIcon = GetNode<Label>("IconBg/PlaceholderIcon");
		_border.Visible = false;
	}

	public void SetWeapon(Weapon weapon)
	{
		_weapon = weapon;
		UpdateDisplay();
	}

	public void SetSelected(bool selected)
	{
		_isSelected = selected;
		_border.Visible = selected;
		Modulate = selected ? new Color(1, 1, 1, 1) : new Color(0.5f, 0.5f, 0.5f, 1);
	}

	private void UpdateDisplay()
	{
		if (_icon == null || _placeholderIcon == null)
			return;

		if (_weapon == null)
		{
			_icon.Visible = false;
			_placeholderIcon.Visible = false;
			return;
		}

		var itemData = _weapon.ItemData;
		if (itemData != null && itemData.Icon != null)
		{
			_icon.Texture = itemData.Icon;
			_icon.Visible = true;
			_placeholderIcon.Visible = false;
		}
		else
		{
			_icon.Visible = false;
			_placeholderIcon.Visible = true;
		}
	}
}
