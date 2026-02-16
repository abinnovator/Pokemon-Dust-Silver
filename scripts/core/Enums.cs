namespace Game.Core{
	public enum LevelLog{
		Debug,
		Info,
		Warning,
		Error,
	}
	#region Characters

	public enum ECharacterAnimationState{
		idle_down,
		idle_up,
		idle_left,
		idle_right,
		walk_down,
		walk_up,
		walk_left,
		walk_right,
		turn_down,
		turn_up,
		turn_left,
		turn_right,

	}
	public enum ECharacterMovement
	{
		WALKING,
		JUMPING
	}
	#endregion


	#region Levels
	public enum LevelName{
		pallet_town,
		pallet_town_ashs_house,
		pallet_town_ashs_house_f1,
		pallet_town_rivals_house,
		pallet_town_pokemon_lab,
		route1,
		virdian_city,
		virdian_city_house,
		virdian_city_arcade,
		virdian_city_pokemart,
		virdian_city_pokecenter,
		virdian_city_gym,
		// Route 2 Done
		route2,
		route2_npc_house,
		route2_gate,
		route2_part2,
		// Pewter City in progress
		pewter_city,
		pewter_city_pokemart,
		pewter_city_pokecenter,
		pewter_city_gym,
		pewter_city_museum,
		pewter_city_museum_f2,
		// Route 3: Done
		route3,
		cerulean_city
	}
	public enum LevelGroups{
		SPAWNPOINTS,
		SCENETRIGGERS
	}
	public enum SignType{
		METAL,
		SNOWY_METAL,
		WOOD,
		SNOWY_WOOD,
		LARGE_WOOD,
		SNOWY_LARGE_WOOD,
		LARGE_METAL,
		SNOWY_LARGE_METAL,
	}
	#endregion
	#region Pokeballs
	public enum PokeballType{
		Closed,
		Open,
	}
	#endregion
	#region Npcs
	public enum NpcAppearance{
		Gardener,
		Worker,
		BugCatcher
	}
	public enum NpcMovementType{
		Static,
		Wander,
		Patrol,
		LookAround
	}
	public enum StoryNpcAppearance 
	{
		// --- Townsfolk (Non-Battlers) ---
		OldMan,
		OldWoman,
		MiddleAgedMan,
		MiddleAgedWoman,
		Youngster,    // The classic shorts-wearing boy
		Lass,         // The classic skirt-wearing girl
		Preschooler,
		FatMan,       // Classic "Technology is incredible!" guy

		// --- Professionals ---
		Nurse,        // Joy-style
		Clerk,        // Shop/Mart worker
		Scientist,    // Lab coats
		Officer,      // Jenny-style
		Doctor,
		Chef,

		// --- Trainer Archetypes ---
		Hiker,
		Camper,
		Picnicker,
		BugCatcher,
		BlackBelt,    // Martial arts
		BattleGirl,
		AceTrainer,   // Professional look
		HexManiac,    // Ghost/Psychic vibe

		// --- Story-Specific ---
		Rival,
		ProfessorOak,
		GymLeader,
		EvilTeamGrunt,
		EvilTeamAdmin,
		Champion,
		Delia
	}
	#endregion
	#region Badges
	public enum Badge
	{
		BOULDER,
		CASCADE,
		THUNDER,
		RAINBOW,
		SOUL,
		MARSH,
		VOLCANO,
		EARTH
	}
	#endregion
	#region SaveGame
	public enum SaveGame
	{
		
	}
	#endregion
	#region Pokemon
	public enum PokemonID
	{
		none,
		bulbasaur,
		ivysaur,
		venusaur,
		charmander,
		charmeleon,
		charizard,
		squirtle,
		wartortle,
		blastoise,
		caterpie,
		metapod,
		butterfree,
		weedle,
		kakuna,
		beedrill,
		pidgey,
		pidgeotto,
		pidgeot,
		rattata,
		raticate,
		spearow,
		fearow,
		ekans,
		arbok,
		pikachu,
		raichu,
		sandshrew,
		sandslash,
		nidoranf,
		nidorina,
		nidoqueen,
		nidoranm,
		nidorino,
		nidoking,
		clefairy,
		clefable,
		vulpix,
		ninetales,
		jigglypuff,
		wigglytuff,
		zubat,
		golbat,
		oddish,
		gloom,
		vileplume,
		paras,
		parasect,
		venonat,
		venomoth,
		diglett,
		dugtrio,
		meowth,
		persian,
		psyduck,
		golduck,
		mankey,
		primeape,
		growlithe,
		arcanine,
		poliwag,
		poliwhirl,
		poliwrath,
		abra,
		kadabra,
		alakazam,
		machop,
		machoke,
		machamp,
		bellsprout,
		weepinbell,
		victreebel,
		tentacool,
		tentacruel,
		geodude,
		graveler,
		golem,
		ponyta,
		rapidash,
		slowpoke,
		slowbro,
		magnemite,
		magneton,
		farfetchd,
		doduo,
		dodrio,
		seel,
		dewgong,
		grimer,
		muk,
		shellder,
		cloyster,
		gastly,
		haunter,
		gengar,
		onix,
		drowzee,
		hypno,
		krabby,
		kingler,
		voltorb,
		electrode,
		exeggcute,
		exeggutor,
		cubone,
		marowak,
		hitmonlee,
		hitmonchan,
		lickitung,
		koffing,
		weezing,
		rhyhorn,
		rhydon,
		chansey,
		tangela,
		kangaskhan,
		horsea,
		seadra,
		goldeen,
		seaking,
		staryu,
		starmie,
		mrmime,
		scyther,
		jynx,
		electabuzz,
		magmar,
		pinsir,
		tauros,
		magikarp,
		gyarados,
		lapras,
		ditto,
		eevee,
		vaporeon,
		jolteon,
		flareon,
		porygon,
		omanyte,
		omastar,
		kabuto,
		kabutops,
		aerodactyl,
		snorlax,
		articuno,
		zapdos,
		moltres,
		dratini,
		dragonair,
		dragonite,
		mewtwo,
		mew
	}
	#endregion


}