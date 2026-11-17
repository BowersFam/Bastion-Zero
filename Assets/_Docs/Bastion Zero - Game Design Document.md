# Bastion Zero — Game Design Document (v0.10)

## 1. High Concept & Premise
* **Title:** Bastion Zero
* **Genre:** 2D Side-Scrolling Action-Adventure (Platforming, Sword Combat, Light Puzzle Solving, Metroidvania Progression)
* **Logline:** Stranded on the once-lush forest planet of Bastion Zero after a colossal insectoid leviathan incinerates his research crew, off-world explorer Nexus takes up an ancient sword and summons the planet's surviving guardians—collecting six keys to awaken the planetary protector Talos before the beast devours the world's remaining ashes.

## 2. World & Lore
* **The Planet — Bastion Zero:** Formerly a peaceful, uncharted world blanketed in dense forests and ancient trees, chosen as the site for an off-world scientific research expedition. Prior to the attack, Nexus's expedition team divided the planet into mapped survey quadrants and sectors. Following the attack, the surface is a ruined, ash-choked wasteland fractured into extreme biomes.
* **The Threat — Kharax, the Cinder-Eater:** A colossal insectoid leviathan that descended from above, unleashed a planet-wide wave of fire that wiped out the planet's surface and Nexus's expedition team, and now actively devours the remaining ashes of Bastion Zero.

## 3. Characters & Summons
* **Nexus (Protagonist):** An off-world explorer and researcher. He arrives on Bastion Zero with his expedition crew, discovers an ancient sword while exploring the peaceful forest, and gains **Wings (Glide)** from a mysterious glowing gem just before Kharax descends. After surviving the firestorm that kills his team, he wields the sword for survival and vengeance, gradually acquiring specialized traversal gear.
* **Summons:**
  * **Vesper (Griffin):** Unlocked in the second half of **Landing Zone A-0** when Nexus finds Vesper trapped in the aftermath of the firestorm and frees it. Fights alongside Nexus as an active AI companion until **Summon Duration** expires.
  * **Archon (Winged Canine):** Unlocked in **Sector C-2 (Research Lab Ruins)**, giving Nexus a major mid-game combat boost. Fights alongside Nexus as an active AI companion until **Summon Duration** expires.
  * **Talos (Planetary Protector):** The monstrous protector of Bastion Zero, sealed behind a lock requiring **6 Keys** scattered across Sectors B-1 through G-6. During regular gameplay, Talos functions as a high-impact **special attack**. In the climax, Talos engages Kharax directly in a kaiju-scale battle while Nexus traverses Kharax's massive body to destroy weak points and turn the tide.

## 4. Gameplay & Progression Loop
* **Core Loop:** Select Survey Sector $\rightarrow$ Navigate Biome Hazards & Light Puzzles $\rightarrow$ Defeat Sector Guardian Boss $\rightarrow$ Claim Talos Key + Traversal Tool (and/or Summon) $\rightarrow$ Unlock Next Sector $\rightarrow$ Backtrack/Explore for Hidden Collectibles.
* **Combat & Traversal:** 2D side-scrolling platforming, melee sword combat, active AI summons (Vesper/Archon), special-attack summon (Talos), and tool-gated environmental puzzles.
* **Progression Stats (Hidden Collectibles):** Health, Power, Shield, Summon Power, and Summon Duration.

## 5. World Structure & Expedition Survey Progression (7 Regions)
1. **Region 1 — Landing Zone A-0 (Primeval Canopy / Prologue):**
   * *Act I (Peaceful Forest):* Opens with Nexus alongside his research crew at their initial survey drop zone. Nexus explores the lush forest, finds his sword (used to clear environmental hazards), and reaches a **mysterious glowing gem** that grants him **Wings (Glide)**.
   * *The Catastrophe:* Immediately upon gaining the Wings, **Kharax** descends and unleashes a planet-wide firestorm, incinerating the forest and wiping out Nexus's crew. *(Permanently transforms the game's title screen menu from the peaceful forest to the burnt, ashen world).*
   * *Act II (Scorched Aftermath):* Nexus survives the firestorm, rescues and frees the trapped griffin **Vesper (Summon)**, and sets out on the quest to find the **6 Keys** to awaken Talos.
2. **Region 2 — Sector B-1 (Scorched Canopy):** Accessed using **Wings (Glide)** across burned chasms and thermal updrafts.
   * *Guardian Boss:* **Pyre-Mantis** (Guards **Key #1**).
   * *Rewards:* **Key #1** + **Wall-Climb**.
3. **Region 3 — Sector C-2 (Research Lab Ruins):** Accessed using **Wall-Climb** to scale shattered facility walls. Nexus uncovers remnants of his lost expedition.
   * *Guardian Boss:* **Aegis-09 Defense Mech** (Guards **Key #2**).
   * *Rewards:* **Key #2** + **Archon (Summon)** + **Poison Filter Mask**.
4. **Region 4 — Sector D-3 (Poisonous Undergrounds):** Accessed using the **Poison Filter Mask** to survive toxic spore tunnels.
   * *Guardian Boss:* **Myco-Wyrm** (Guards **Key #3**).
   * *Rewards:* **Key #3** + **Heat Pack**.
5. **Region 5 — Sector E-4 (Molten Peak):** Accessed using the **Heat Pack** to endure extreme volcanic temperatures.
   * *Guardian Boss:* **Igneous Goliath** (Guards **Key #4**).
   * *Rewards:* **Key #4** + **Spiked Boots** *(provides traction on steep volcanic slopes and prevents sliding on ice)*.
6. **Region 6 — Sector F-5 (Frozen Waste):** Accessed using **Spiked Boots** to traverse slick glacial terrain without sliding.
   * *Guardian Boss:* **Cryo-Stalker** (Guards **Key #5**).
   * *Rewards:* **Key #5** + **Explosives** *(blasts open sealed rock, ice, and collapsed dam barriers)*.
7. **Region 7 — Sector G-6 (Murky Waterways):** Accessed using **Explosives** to breach collapsed stone barriers and flooded ruins.
   * *Guardian Boss:* **Abyssal Brood-Matriarch** (Guards **Key #6** at the threshold of Talos's sanctum).
   * *Rewards:* **Key #6** + **Awakens Talos** $\rightarrow$ Triggers the final confrontation against **Kharax**.

## 6. Bestiary & Guardian Bosses *(Working Concepts)*
### A. Standard Enemy Factions
1. **Kharax's Insectoid Brood (Chitinous, glowing ember-veined insects dropped during the invasion):**
   * *Cinder-Mite (Swarmer):* Fast, low-HP crawlers that rush Nexus in packs; easily cleared with wide sword slashes or Vesper's dive attacks.
   * *Ash-Wasp (Aerial):* Hovering ranged units that spit molten globules; encourages mid-air sword combos, gliding, or aerial summon assists.
   * *Shell-Brute (Heavy):* Armored beetle-like tanks immune to frontal strikes; forces Nexus to jump/glide behind them or use Archon/Explosives to break their guard.
   * *Biome Variants:* Brood adapt to their sectors—sprouting venom sacs in **Sector D-3**, hardened obsidian shells in **Sector E-4**, and frost-chitin in **Sector F-5**.
2. **Frenzied Native Wildlife (Survivors driven aggressive by the catastrophe):**
   * *Ember-Stag / Scorched Prowler (Sector B-1):* Fast melee beasts that telegraph lunges across charred branches.
   * *Spore-Toad / Mire-Lurker (Sectors D-3 & G-6):* Ambush predators that camouflage in toxic pools or murky water before tongue-lashing or leaping out.
   * *Rime-Wolf (Sector F-5):* Pack hunters with high traction on ice, testing Nexus's Spiked Boots footwork.
3. **Malfunctioning Expedition Tech (Corrupted or haywire gear from Nexus's own crew):**
   * *Survey Drone (Sectors C-2 & F-5):* Floating scanner bots whose searchlights turn red on detection, firing laser bursts.
   * *Auto-Turret / Hazard Loader (Sector C-2):* Stationary wall-mounted defenses and heavy mining rigs gone haywire inside the ruined labs.

### B. Sector Guardian Bosses (Key Keepers)
* **Sector B-1 Boss — Pyre-Mantis:** A towering ash-cloaked insectoid predator that leaps between crumbling branches and unleashes sweeping fire-wave slashes, teaching players to use **Wings (Glide)** and **Vesper** in boss combat.
* **Sector C-2 Boss — Aegis-09 Defense Mech:** A heavy expedition security walker corrupted by brood spores; uses shield barriers and laser sweeps, requiring **Wall-Climb** to reach elevated platforms and strike its exposed power core.
* **Sector D-3 Boss — Myco-Wyrm:** A massive burrowing centipede/fungal beast that erupts from toxic spore pools and floods the arena with poison clouds, rewarding aggressive **Archon** summons.
* **Sector E-4 Boss — Igneous Goliath:** A crustacean-like magma brood coated in cooled volcanic rock; players must shatter its molten armor plates while dodging falling lava boulders.
* **Sector F-5 Boss — Cryo-Stalker:** An agile, frost-armored apex predator that dashes across slick glacial arenas and summons ice pillars, testing the player's **Spiked Boots** traction.
* **Sector G-6 Boss — Abyssal Brood-Matriarch:** A colossal amphibious insectoid guarding the submerged seal to Talos's sanctum; uses rising water levels, tentacle/claw slams, and **Explosive**-vulnerable armored nodes.

## 7. Endgame Climax: The Battle Against Kharax
* **Kaiju-Scale Dual-Layer Encounter:** Upon awakening Talos with all 6 Keys, Talos confronts Kharax in a massive titan battle.
* **Nexus's Role:** Nexus uses his full suite of traversal tools (**Wings, Wall-Climb, Heat Pack, Spiked Boots, Explosives**) to platform directly across Kharax's colossal, shifting insectoid carapace—fending off defending brood, destroying armored weak points, and exposing vulnerabilities so Talos can land decisive strikes to defeat the Cinder-Eater.

## 8. Visual Style & Presentation
* **Art Direction — HD-2D:** Crisp, expressive 2D pixel-art character sprites, summons, and bosses set inside richly layered 3D environments (inspired by *Octopath Traveler* and *Final Fantasy Resonance*).
* **Dynamic Title Screen:** Opens on a serene, sun-lit HD-2D diorama of Bastion Zero's peaceful forest prior to the prologue catastrophe. Once Kharax lands at the end of Landing Zone A-0's first act, the main menu permanently shifts to reflect the scorched, ash-covered world.
* **Lighting & Atmosphere:** Modern post-processing—tilt-shift depth of field, real-time dynamic shadows, bloom, and volumetric particle effects—used to accentuate biome contrast (sun-dappled canopy in the prologue vs. glowing embers, toxic bioluminescence, volcanic heat haze, and reflective water shaders).
* **Scale & Spectacle:** Uses 2.5D foreground/background depth to frame colossal entities—making **Talos** and **Kharax** tower across the background plane before intersecting directly with Nexus's 2D platforming plane.