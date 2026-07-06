extends CanvasLayer

@onready var select_arrow = $Control/NinePatchRect/TextureRect
@onready var menu = $Control

var party_screen: Node = null
var bag: Node = null
var card: Node = null

enum ScreenLoaded {NOTHING, JUST_MENU, PARTY_SCREEN, BAG, CARD}
var screen_loaded = ScreenLoaded.NOTHING

var selected_option: int = 0
const MAX_OPTIONS = 6

func _ready() -> void:
	menu.visible = false
	update_selection()

func update_selection():
	if select_arrow:
		# Wrap the selection between 0 and 5
		selected_option = posmod(selected_option, MAX_OPTIONS)
		select_arrow.position.y = 8 + (selected_option * 14)

func _unhandled_input(event):
	match screen_loaded:
		ScreenLoaded.NOTHING:
			if event.is_action_pressed("menu"):
				menu.visible = true
				screen_loaded = ScreenLoaded.JUST_MENU
				Signals.emit_signal("MenuOpen", true)
				get_viewport().set_input_as_handled()
		
		ScreenLoaded.JUST_MENU:
			if event.is_action_pressed("menu"):
				close_menu()
			
			elif event.is_action_pressed("ui_down"):
				selected_option += 1
				update_selection()
				
			elif event.is_action_pressed("ui_up"):
				selected_option -= 1
				update_selection()
			
			elif event.is_action_pressed("z"): # Confirm Action
				handle_menu_selection()
				get_viewport().set_input_as_handled()

		# Handle "Back" logic for open sub-screens
		ScreenLoaded.PARTY_SCREEN, ScreenLoaded.BAG, ScreenLoaded.CARD:
			if event.is_action_pressed("c"): # Back Action
				close_sub_screen()
				get_viewport().set_input_as_handled()

func handle_menu_selection():
	match selected_option:
		0: # Party
			if party_screen == null:
				party_screen = load("res://scenes/ui/PokemonPartyScreen.tscn").instantiate()
				add_child(party_screen)
			screen_loaded = ScreenLoaded.PARTY_SCREEN
		1: # Bag
			if bag == null:
				bag = load("res://scenes/ui/bag.tscn").instantiate()
				add_child(bag)
			screen_loaded = ScreenLoaded.BAG
		2: # Quit (Based on your logic mapping)
			SaveManager.QuitGame()
		3: # Trainer Card
			if card == null:
				card = load("res://scenes/ui/trainer_card.tscn").instantiate()
				add_child(card)
			screen_loaded = ScreenLoaded.CARD
		4: # Exit Menu
			close_menu()


func close_sub_screen():
	if party_screen:
		party_screen.queue_free()
		party_screen = null
	if bag:
		bag.queue_free()
		bag = null
	if card:
		card.queue_free()
		card = null
	
	screen_loaded = ScreenLoaded.JUST_MENU

func close_menu():
	menu.visible = false
	screen_loaded = ScreenLoaded.NOTHING
	Signals.emit_signal("MenuOpen", false)
