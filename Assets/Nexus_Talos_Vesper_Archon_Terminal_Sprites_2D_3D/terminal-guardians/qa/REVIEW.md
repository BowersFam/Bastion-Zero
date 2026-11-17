# Sprite export review

The eight generated source sheets were visually reviewed for character identity and all eleven action rows. Their transparent row separators were checked before extraction. Equal-height source rows would cut into feet or neighboring actions, so the exporter uses the detected separators.

The terminal exports were independently reviewed for death, flight and all three attacks in both styles. Death ends prone. The attacks change pose and effects. Body silhouettes remain intact, and the PNG exports match the indexed JSON exactly. A few source attack-effect fragments read as energy sparks at this resolution.

All 88 clips contain four unique frames. The structural validator found no empty frames or export-canvas edge contacts. All 352 frames decoded and rendered at 32, 48 and 64 terminal columns. The browser preview's JavaScript passed syntax validation. The standard-library terminal player completed a one-shot playback smoke check.

Archon's flight row was raised six logical pixels after review so its straight-legged wing-flap poses read as airborne. This is a terminal-canvas placement adjustment; the original generated sheet remains preserved.

These checks cover the supplied files and renderer. They do not establish compatibility with every terminal font, operating system or game engine. Native Windows GUI playback was not independently observed in this session.
