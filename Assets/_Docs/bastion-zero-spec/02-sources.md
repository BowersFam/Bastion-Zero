# Bastion Zero spec — Unified source list

> Part of the Bastion Zero clean-room mechanics spec v3.2 (14 Nov 2026). Reference only. Public sources behind [sourced] and [community-measured] values. Not needed to implement; use it to trace a value.
> Precedence: user decisions (file 01) > owning system > later system. Tags: [sourced], [community-measured], [estimated] (file 01).

---

### 0.2 Unified source list (de-duplicated)
One row per source code. Where several system files described the same code differently, the longest description is kept and the merge is noted; all URLs given for a code in any file are kept. "Cited by" lists the systems whose source keys include the code.

| Code | Source | URL(s) | Cited by |
|---|---|---|---|
| S1 | Game Developer, "Game Design Deep Dive: Re-imagining the double-jump in The Messenger" (T. Boulanger, 2019) | https://www.gamedeveloper.com/design/game-design-deep-dive-re-imagining-the-double-jump-in-i-the-messenger-i- | SYS1 |
| S2 | The Geekly Grind, "Perfecting The Ninja Way" (dev interview, 2018) | https://www.thegeeklygrind.com/all-posts/perfecting-the-ninja-way-a-conversation-with-the-messenger-development-team | SYS1 |
| S3 | Kotaku dev interview (TM): attacking never slows running; jumping is never delayed by an attack (merged 3 descriptions; longest kept) | https://kotaku.com/tight-controls-define-this-retro-homage-to-ninja-gaiden-1822282879 | SYS1, SYS2, SYS3 |
| S4 | The Messenger Wiki, Shop Upgrades page (catalogue, prices shown in the in-game shop, HP 5→8, charge pool, checkpoint-heal upgrade, penalty upgrade) (merged 4 descriptions; longest kept) | https://the-messenger.fandom.com/wiki/Cloudstep · /wiki/Climbing_Claws · /wiki/Rope_Dart · /wiki/Wingsuit · /wiki/Shop_Upgrades · https://the-messenger.fandom.com/wiki/Shop_Upgrades | SYS1, SYS2, SYS3, SYS6 |
| S5 | TheMessengerRTA Wiki (speedrun community): General Info, Out-of-Bounds Techniques (merged 2 descriptions; longest kept) | https://themessengerrta.fandom.com/wiki/General_Info · https://themessengerrta.fandom.com/wiki/Out-of-Bounds_Techniques | SYS1, SYS2 |
| S6 | Steam Community guide, "100% Completion Guide & Walkthrough" (merged 3 descriptions; longest kept) | https://steamcommunity.com/sharedfiles/filedetails/?id=1643339519 | SYS1, SYS2, SYS3, SYS6 |
| S7 | GameFAQs TM guide by KumihoStar (first-boss HP and pattern thresholds) (merged 3 descriptions; longest kept) | https://gamefaqs.gamespot.com/pc/230087-the-messenger/faqs/77725/general-information | SYS1, SYS2, SYS3, SYS4 |
| S8 | The Messenger official FAQ | https://themessengergame.com/faq/ | SYS1 |
| S9 | Neoseeker walkthrough (glide input and updrafts) | https://www.neoseeker.com/the-messenger-2018/walkthrough/Howling_Grotto | SYS1 |
| S10 | aigamedev.ai, "Measured movement: Celeste, Hollow Knight, Shovel Knight" (frame tracking of 60 fps footage) (merged 3 descriptions; longest kept) | https://aigamedev.ai/skills/platformer-feel/references/measured-movement.md | SYS1, SYS3, SYS5 |
| S11 | Hollow Knight Wiki: dash page, wall-cling page, double-jump page, damage-recovery charm page, weapon page (pogo) (merged 4 descriptions; longest kept) | https://hollowknight.wiki/w/Mothwing_Cloak · /w/Mantis_Claw · /w/Monarch_Wings · /w/Stalwart_Shell · /w/Nail · https://hollowknight.wiki/w/Mantis_Claw · https://hollowknight.wiki/w/Nail | SYS1, SYS2, SYS3, SYS6 |
| S12 | Gaming StackExchange, "How does the invincibility frame system work in Hollow Knight?" (merged 2 descriptions; longest kept) | https://gaming.stackexchange.com/questions/413676 | SYS1, SYS2 |
| S13 | Hollow Knight Speedrunning Wiki, "Hazard warp" (merged 2 descriptions; longest kept) | https://hollow-knight-speedrunning.fandom.com/wiki/Hazard_warp | SYS1, SYS2 |
| S14 | Official Dead Cells Wiki: Mechanics (rolling, climbing, falling, air stall, dive attack) (merged 3 descriptions; longest kept) | https://deadcells.wiki.gg/wiki/Mechanics | SYS1, SYS2, SYS3, SYS4 |
| S15 | Official Dead Cells Wiki: Controls (advanced maneuvers) (merged 2 descriptions; longest kept) | https://deadcells.wiki.gg/wiki/Controls | SYS1, SYS2, SYS3 |
| S16 | Speedrun.com, Dead Cells "List of Current Tech" | https://www.speedrun.com/deadcells/guides/oo8vv | SYS1 |
| S17 | r/deadcells thread on ledge-grab invulnerability (merged 2 descriptions; longest kept) | https://www.reddit.com/r/deadcells/comments/somwau/ | SYS1, SYS2 |
| S18 | Rogue Ranker, HK wall-cling explainer (restates the wiki) | https://rogueranker.com/how-to-get-mantis-claw-hollow-knight/ | SYS1 |
| S19 | Official DC Wiki: Hazards (incl. version history) | https://deadcells.wiki.gg/wiki/Hazards | SYS2 |
| S20 | Official DC Wiki: Shields | https://deadcells.wiki.gg/wiki/Shields | SYS2, SYS3 |
| S21 | Official DC Wiki: Stats | https://deadcells.wiki.gg/wiki/Stats | SYS2, SYS3 |
| S22 | Official DC Wiki: Permadeath | https://deadcells.wiki.gg/wiki/Permadeath | SYS2, SYS6 |
| S23 | Official DC Wiki: Biomes (scaling section) | https://deadcells.wiki.gg/wiki/Biomes | SYS2 |
| S24 | Hollow Knight Wiki: protagonist page (health section) | https://hollowknight.wiki/w/The_Knight | SYS2, SYS6 |
| S25 | Hollow Knight Wiki: death-remnant enemy page | https://hollowknight.wiki/w/Shade | SYS2, SYS6 |
| S26 | Hollow Knight Wiki: a late boss page (2-unit contact damage) | https://hollowknight.wiki/w/Failed_Champion | SYS2 |
| S27 | Ludonauta devlog, "The Hidden Genius Behind Hollow Knight's Terrain Design" | https://ludonauta.itch.io/platformer-essentials/devlog/1084669/the-hidden-genius-behind-hollow-knights-terrain-design | SYS2 |
| S28 | ResetEra forum thread, HK terrain geometry | https://www.resetera.com/threads/how-to-enjoy-hollow-knight.124973/page-3 | SYS2 |
| S29 | Atomic Bob-Omb blog, "Hollow Knight & Knockback" | https://atomicbobomb.home.blog/2019/02/05/hollow-knight-knockback/ | SYS2, SYS3 |
| S30 | Stack Overflow, frame-by-frame observations of HK melee (merged 2 descriptions; longest kept) | https://stackoverflow.com/questions/58929965/cloning-melee-combat-from-hollow-knight | SYS2, SYS3 |
| S31 | Game Developer, "How does 'The Messenger' make you a master?" | https://www.gamedeveloper.com/design/how-does-the-messenger-make-you-a-master- | SYS2 |
| S32 | Neoseeker TM walkthroughs (two levels with crushers, moving and collapsing platforms) | https://www.neoseeker.com/the-messenger-2018/walkthrough/Catacombs · /walkthrough/Underworld | SYS2 |
| S33 | Speedrun.com TM guide (includes "spike seeding") | https://www.speedrun.com/the_messenger/guides/1sjxb | SYS2 |
| S34 | Steam DC discussion, "Collision detection (Enemy bumps)" | https://steamcommunity.com/app/588650/discussions/0/1457328392105850726/ | SYS2 |
| S35 | r/deadcells, "needs temporary invulnerability after being hit" | https://www.reddit.com/r/deadcells/comments/6n5trx/ | SYS2 |
| S36 | kosmender.com, DC final-boss guide (body contact on charge and slam) | https://www.kosmender.com/hand-of-the-king | SYS2 |
| S37 | 80.lv, "Interview With the Developers of Dead Cells" (hit-stop on crits) | https://80.lv/articles/interview-with-the-developers-of-dead-cells | SYS3 |
| S38 | teemo.dev, DC "Game Feel & Juice". Used only for its summary of the 2019 GDC talk by DC's designer and the accessibility toggles; no game-file values. (merged 2 descriptions; longest kept) | https://teemo.dev/game-design/dead-cells/systems/game-feel-and-juice/ | SYS3, SYS5 |
| S39 | Ars Technica "War Stories" video with DC's designer (input leniency) | https://www.youtube.com/watch?v=Uv5NwboDDhk | SYS3 |
| S40 | Official DC Wiki: Status effects | https://deadcells.wiki.gg/wiki/Status_effects | SYS3 |
| S41 | Official DC Wiki: Gear (slots, melee/ranged/shield mechanics, skill tables) | https://deadcells.wiki.gg/wiki/Weapons | SYS3 |
| S42 | Official DC Wiki: starter-sword page (combo rate; provenance unclear) | https://deadcells.wiki.gg/wiki/Rusty_Sword | SYS3 |
| S43 | Nestor's Long Stuff, "Dead Cells: Movement" (critique: cancels and roll) | https://nestorslongstuff.home.blog/2018/12/14/dead-cells-movement/ | SYS3 |
| S44 | HK Wiki: rest-point page (what resting does; count; tolls) (merged 3 descriptions; longest kept) | https://hollowknight.wiki/w/Bench_(Hollow_Knight) | SYS4, SYS5, SYS6 |
| S45 | HK Wiki: Combat (boss stagger rules) | https://hollowknight.wiki/w/Combat_(Hollow_Knight) | SYS4 |
| S46 | HK Wiki: final-boss page (phase thresholds, stagger and combo values; provenance unclear) | https://hollowknight.wiki/w/The_Hollow_Knight | SYS4 |
| S47 | HK Wiki: armoured-boss page (armour-pool stagger) | https://hollowknight.wiki/w/False_Knight | SYS4 |
| S48 | Steam Community, HK discussion "stop enemies respawn" (weak enemies respawn on room re-entry) | https://steamcommunity.com/app/367520/discussions/0/1693795812301576401/ | SYS4 |
| S49 | Official DC Wiki: Enemies (elite rules, behaviours) | https://deadcells.wiki.gg/wiki/Enemies | SYS4 |
| S50 | Official DC Wiki: Bosses (status resistance) | https://deadcells.wiki.gg/wiki/Bosses | SYS4 |
| S51 | Official DC Wiki: a boss page (phases at 75% and 50% with a push-back roar) | https://deadcells.wiki.gg/wiki/The_Giant | SYS4 |
| S52 | Official DC Wiki: a boss page (summon phase, invulnerability, charge into pits, timed fuses) | https://deadcells.wiki.gg/wiki/The_Hand_of_the_King | SYS4 |
| S53 | Official DC Wiki: a boss page (speed-up at 50%) | https://deadcells.wiki.gg/wiki/The_Scarecrow | SYS4 |
| S54 | Reddit r/deadcells, parry-tips thread (attack sign; variable sign-to-hit delay) | https://www.reddit.com/r/deadcells/comments/vur0g8/can_someone_give_me_tips_on_parrying/ | SYS4 |
| S55 | Reddit r/gamedesign, "What draws your attention" (attack sign; off-screen edge indicator) | https://www.reddit.com/r/gamedesign/comments/wyfh5l/what_draws_your_attention/ | SYS4 |
| S56 | teemo.dev DC "UX, UI & Accessibility". Used only for its list of options menus (attack-sign size, assist sliders); no game-file values. | https://teemo.dev/game-design/dead-cells/systems/ux-ui-and-accessibility/ | SYS4 |
| S57 | GameSpew, The Messenger review (off-screen respawn) (merged 2 descriptions; longest kept) | https://www.gamespew.com/2018/08/the-messenger-review/ | SYS4, SYS5 |
| S58 | Chris Davis, "The Messenger (Switch)" blog review (respawn tied to an off-screen spawn point) (merged 2 descriptions; longest kept) | https://cdavisgames.wordpress.com/2018/12/03/the-messenger-switch/ | SYS4, SYS5 |
| S59 | Neoseeker TM walkthrough (lock-in rooms with waves; a boss knock-down stun window) | https://www.neoseeker.com/the-messenger-2018/walkthrough/Catacombs | SYS4 |
| S60 | The Centurion Report, DC boss guide (phase change at about 70% and 50%) | https://thecenturionreport.com/dead-cells-the-time-keeper-boss-guide-kill-the-clock/ | SYS4 |
| S61 | PC Gamer, "How to design a great Metroidvania map" (Team Cherry interview, 2017) | https://www.pcgamer.com/how-to-design-a-great-metroidvania-map/ | SYS5 |
| S62 | Game Informer, "The Making Of Hollow Knight" (2018) | https://gameinformer.com/2018/10/15/the-making-of-hollow-knight | SYS5 |
| S63 | Kotaku Splitscreen interview with Team Cherry (2018, video) | https://www.youtube.com/watch?v=6bQP2rmNBpI | SYS5 |
| S64 | Power Up Gaming, Hollow Knight developer interview (2015) | https://powerupgaming.co.uk/2015/11/20/developer-interview-hollow-knight-promising-platformer-with-lots-to-hide/ | SYS5 |
| S65 | HK Wiki: map and mapping-tool page | https://hollowknight.wiki/w/Map_and_Quill_(Hollow_Knight) | SYS5, SYS6 |
| S66 | HK Wiki: fast-travel page (station tolls, call bell, trams) (merged 2 descriptions; longest kept) | https://hollowknight.wiki/w/Stag_Station · https://hollowknight.wiki/w/Fast_Travel_(Hollow_Knight) (redirect target of /w/Stag_Station) | SYS5, SYS6 |
| S67 | Hollow Knight Interactive Map, fan guide to the map system | https://hollowknightinteractivemap.com/guides/map-and-quill | SYS5 |
| S68 | Reddit r/HollowKnight, room count thread (hand count, corrected) | https://www.reddit.com/r/HollowKnight/comments/1f7ki9x/how_many_rooms_are_there_in_hollow_knight/ | SYS5 |
| S69 | Reddit r/HollowKnight, "How many rooms?" (separate hand count) | https://www.reddit.com/r/HollowKnight/comments/vma064/how_many_rooms/ | SYS5 |
| S70 | HK Wiki: world overview page (used only to count named areas) | https://hollowknight.wiki/w/Hallownest | SYS5 |
| S71 | HollowKnight.LoadNormalizer README (load-phase timings measured on players' machines with a race mod) | https://github.com/homothetyhk/HollowKnight.LoadNormalizer | SYS5 |
| S72 | ComicBook.com, interview with Sabotage Studio (TM structure, screen-by-screen design) | https://comicbook.com/gaming/news/the-messenger-interview-sabotage-studios-nintendo-switch-pc/ | SYS5 |
| S73 | The Young Folks, PAX East 2019 interview with TM's creative director (portals in the open-world phase) (merged 2 descriptions; longest kept) | https://www.theyoungfolks.com/video-games/131649/the-messengers-thierry-boulanger-on-dlc-update-picnic-panic-pax-east-2019/ | SYS5, SYS6 |
| S74 | GamesBeat, TM hands-on interview (shift point; revisiting levels) | https://gamesbeat.com/ninja-gaiden-throwback-the-messenger-is-great-and-makes-me-feel-guilty/ | SYS5 |
| S75 | Destructoid, interview with TM's creator (campaign length) | https://www.destructoid.com/we-talk-ninja-flips-metroidvanias-and-the-25-year-journey-of-the-messenger-with-creator-thierry-boulanger/ | SYS5, SYS6 |
| S76 | Official DC Wiki: Biomes (biome counts, exits, map views) (merged 2 descriptions; longest kept) | https://deadcells.wiki.gg/wiki/Biomes | SYS5, SYS6 |
| S77 | Official DC Wiki: Runes and upgrades (permanent path unlocks; permanent upgrades bought with the meta currency, with prices; gold kept on death) (merged 2 descriptions; longest kept) | https://deadcells.wiki.gg/wiki/Runes | SYS5, SYS6 |
| S78 | Itay Keren, "Scroll Back: The Theory and Practice of Cameras in Side-Scrollers" (GDC 2015 talk, article version) | https://www.gamedeveloper.com/design/scroll-back-the-theory-and-practice-of-cameras-in-side-scrollers | SYS5 |
| S79 | The Messenger Wiki: currency page (sources, sinks, farming by reload) | https://the-messenger.fandom.com/wiki/Time_Shards | SYS6 |
| S80 | The Messenger Wiki: death-penalty follower character page | https://the-messenger.fandom.com/wiki/Quarble | SYS6 |
| S81 | The Messenger Wiki: shop location page (reached from shop checkpoints) | https://the-messenger.fandom.com/wiki/The_Shop | SYS6 |
| S82 | The Messenger Wiki: challenge-room collectible page (45 collectibles; reward) | https://the-messenger.fandom.com/wiki/Power_Seals | SYS6 |
| S83 | Hollow Knight Wiki: currency page (denominations, tolls, merchant totals, sources) | https://hollowknight.wiki/w/Geo | SYS6 |
| S84 | Hollow Knight Wiki: charms page (notches, equip only at rest points, overcharm) | https://hollowknight.wiki/w/Charms | SYS6 |
| S85 | Hollow Knight Wiki: completion page (percentage weights) | https://hollowknight.wiki/w/Completion_(Hollow_Knight) | SYS6 |
| S86 | Hollow Knight Wiki: max-health fragment page (4 per unit; shop prices) | https://hollowknight.wiki/w/Mask_Shard_(Hollow_Knight) | SYS6 |
| S87 | Hollow Knight Wiki: soul meter page (healing spends a third of the meter) | https://hollowknight.wiki/w/Soul | SYS6 |
| S88 | Official DC Wiki: Pickups (meta currency, gold, food; lost on death). Its drop-chance rows are excluded. | https://deadcells.wiki.gg/wiki/Cells | SYS6 |
| S90 | Official DC Wiki: the forge NPC page (permanent investment of meta currency) | https://deadcells.wiki.gg/wiki/Forge | SYS6 |
| S91 | Official DC Wiki: the meta-currency NPC page (located in passages between biomes; purchases carry over) | https://deadcells.wiki.gg/wiki/The_Collector | SYS6 |
| S92 | Game Rant, "How Long it Takes to Beat Hollow Knight…" (HowLongToBeat player polls) | https://gamerant.com/hollow-knight-how-long-to-beat-all-dlc-collectibles-silksong/ | SYS6 |
| S93 | MoonCellar, The Messenger page (HowLongToBeat figures) | https://mooncellar.space/games/the-messenger | SYS6 |

**Citations kept from reference comparisons only.** These codes support comparison rows or design reasons in the system files but no merged value in §3. They are kept here so each original citation still has a home.

| Code | Claim as cited in the system file | Cited in |
|---|---|---|
| S17 | DC players report short invulnerability during a ledge climb, shorter than a roll's; unmeasured | SYS1 §5; SYS2 §8 |
| S18 | HK: holding both directions while sliding drops at normal fall speed (with S11) | SYS1 §5 |
| S22 | DC permadeath: gold kept up to 2,500 [sourced] with meta upgrades; basis of exclusion PE1 | SYS2 §9; SYS6 §1 |
| S23 | DC enemy damage and HP scale by biome tier and difficulty; constants excluded (E7) | SYS2 §5; SYS3 §1 |
| S27 | HK has no one-way platforms; all terrain solid; absence is the gate | SYS2 §2 |
| S28 | HK geometry is right-angled rectangles (forum observation); basis of OQ-L3 | SYS2 §3, OQ1 |
| S29 | HK pushes hero and enemy apart on each connect (with S30); not used (CF-X5) | SYS2 §7; SYS3 §6 |
| S30 | HK hit/recoil separation on every connect, observed frame by frame | SYS2 §1, §7; SYS3 §6 |
| S31 | TM has one-way platforms and uses a forced drop-through as a gate; crushers arrive mid-game | SYS2 §2, §4 |
| S32 | TM rideable moving platforms over spikes, collapsing platforms, duckable spike balls | SYS2 §4, §10 |
| S34 | DC enemy bodies shove the hero, including off ledges; reason actor-to-actor collision is off | SYS2 §1, §6 |
| S39 | Listed in SYS3's source key (DC designer on input leniency); no body citation in SYS3 either | SYS3 §0 |
| S43 | DC roll cancels the player's own attack recovery (with S38) | SYS3 §3 |
| S64 | HK built as one continuous interconnected world (with S61) | SYS5 §2.1 |
| S87 | HK heals by spending a third of a meter; considered for the flask and not used | SYS6 §2.1, §8 |
| S90 | DC forge investment in random item quality; basis of exclusion PE4 | SYS6 §0, §1 |
| S91 | DC meta-currency NPC sits in passages between biomes; basis of exclusion PE2 | SYS6 §1, §2.1 |

**De-duplication notes**
- 92 codes are in use: S1–S88 and S90–S93. **S89 is retired**: SYS6 v2 merged it into S88 and no longer lists it.
- 21 codes appear in more than one system's source key with different wording; they are merged above, longest description kept.
- Two pages appear under two codes each, because different sections were used: https://deadcells.wiki.gg/wiki/Biomes (S23 scaling section, S76 counts and map views) and the Neoseeker TM walkthrough page (S32, S59). Both codes are kept so every original citation still resolves.
- Multi-page codes: S4 (TM wiki: several pages), S5 (two speedrun pages), S11 (HK wiki: several pages), S66 (fast-travel page plus its redirect target).

