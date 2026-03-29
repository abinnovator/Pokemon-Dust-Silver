extends CanvasLayer

@onready var select_arrow = $Control/NinePatchRect/TextureRect
@onready var menu = $Control
var party_screen: Node = null
var bag: Node=null

enum ScreenLoaded {NOTHING, JUST_MENU, PARTY_SCREEN, BAG}
var screen_loaded = ScreenLoaded.NOTHING

var selected_option: int = 0

func _ready() -> void:
	menu.visible = false
	# Initialize the arrow position
	if select_arrow:
		select_arrow.position.y = 8 + (selected_option % 6) * 14

func _unhandled_input(event):
	match screen_loaded:
		ScreenLoaded.NOTHING:
			if event.is_action_pressed("menu"):
				
				menu.visible = true
				screen_loaded = ScreenLoaded.JUST_MENU
		
		ScreenLoaded.JUST_MENU:
			if event.is_action_pressed("menu"):
				menu.visible = false
				screen_loaded = ScreenLoaded.NOTHING
				
			elif event.is_action_pressed("ui_down"):
				selected_option += 1
				if select_arrow:
					select_arrow.position.y = 8 + (selected_option % 6) * 14
					
			elif event.is_action_pressed("ui_up"):
				if selected_option == 0:
					selected_option = 5
				else:
					selected_option -= 1
				if select_arrow:
					select_arrow.position.y = 8 + (selected_option % 6) * 14
			elif event.is_action("z") and event.pressed:
				if selected_option == 0:
					if party_screen == null:
						party_screen = load("res://scenes/ui/PokemonPartyScreen.tscn").instantiate()
						add_child(party_screen)
					screen_loaded = ScreenLoaded.PARTY_SCREEN
					get_viewport().set_input_as_handled()
				elif selected_option == 1:
					if bag == null:
						bag = load("res://scenes/ui/bag.tscn").instantiate()
						add_child(bag)
					screen_loaded = ScreenLoaded.BAG
					get_viewport().set_input_as_handled()
				elif selected_option == 2:
					SaveManager.QuitGame();

		ScreenLoaded.PARTY_SCREEN:
			if event.is_action("c") and event.pressed:
				if party_screen != null:
					party_screen.queue_free()
					party_screen = null
				elif bag != null:
					bag.queue_free()
					bag = null
				screen_loaded = ScreenLoaded.JUST_MENU
				get_viewport().set_input_as_handled()
