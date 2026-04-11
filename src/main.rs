use bevy::prelude::*;
use rand::Rng;

// ---------------------------------------------------------------------------
// Constants – matched to the original C++ / SFW game
// ---------------------------------------------------------------------------
const WINDOW_WIDTH: f32 = 800.0;
const WINDOW_HEIGHT: f32 = 600.0;

// Physics (original uses per-frame gravity at ~60 fps)
const GRAVITY: f32 = -9.8;
const JUMP_MULTIPLIER: f32 = 30.0;
const PLAYER_MOVE_FORCE: f32 = 5.0;
const PLAYER_DRAG_GROUNDED: f32 = 3.0;
const PLATFORM_SPEED: f32 = 1.5;
const PLATFORM_SPEED_L2: f32 = 2.0;

// Mario sprite-sheet: 357×66 px, 21 columns × 2 rows
const MARIO_COLS: u32 = 21;
const MARIO_ROWS: u32 = 2;
const MARIO_TILE_W: u32 = 17; // 357 / 21
const MARIO_TILE_H: u32 = 33; // 66 / 2

// ---------------------------------------------------------------------------
// Components
// ---------------------------------------------------------------------------

#[derive(Component)]
struct Player {
    gravity: f32,
    is_grounded: bool,
    is_on_platform: bool,
    end_game: bool,
}

#[derive(Component)]
struct Velocity(Vec2);

#[derive(Component)]
struct Impulse(Vec2);

#[derive(Component)]
struct Force(Vec2);

#[derive(Component)]
struct Mass(f32);

#[derive(Component)]
struct Drag(f32);

/// Half-extents factor relative to the sprite's custom_size.
/// 0.5 means collision box == full sprite, 0.42 for ground, etc.
#[derive(Component)]
struct ColliderExtents(f32);

#[derive(Component)]
struct SpriteSize(Vec2);

#[derive(Component)]
struct Ground;

#[derive(Component)]
struct Goal;

#[derive(Component)]
struct Platform;

#[derive(Component, Clone)]
enum PlatformKind {
    Static,
    Horizontal {
        min_x: f32,
        max_x: f32,
        speed: f32,
        moving_right: bool,
    },
    Vertical {
        min_y: f32,
        max_y: f32,
        speed: f32,
        moving_up: bool,
    },
    UpRight {
        min_x: f32,
        max_x: f32,
        speed: f32,
        moving_right: bool,
        moving_up: bool,
    },
    UpLeft {
        min_x: f32,
        max_x: f32,
        speed: f32,
        moving_right: bool,
        moving_up: bool,
    },
    MultiDir {
        min_x: f32,
        mid_x: f32,
        max_x: f32,
        speed: f32,
        moving_right: bool,
        moving_up: bool,
        moving_left: bool,
        #[allow(dead_code)]
        moving_down: bool,
    },
}

#[derive(Component)]
struct AnimState {
    min: usize,
    max: usize,
    idx: usize,
    timer: f32,
    start_timer: f32,
    facing_right: bool,
}

#[derive(Component)]
struct Firework;

#[derive(Component)]
struct WinText;

/// Marker for entities that belong to the current level and should be despawned
/// on level transition.
#[derive(Component)]
struct LevelEntity;

#[derive(Resource)]
struct CurrentLevel(u32);

/// Tracks which platform (if any) the player is riding so we can apply its
/// velocity to the player each frame (replicates the original's parenting).
#[derive(Resource, Default)]
struct PlayerPlatformLink {
    platform_velocity: Vec2,
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/// AABB overlap test.  Returns `Some((push_axis, penetration))` on collision.
fn aabb_overlap(
    pos_a: Vec2,
    half_a: Vec2,
    pos_b: Vec2,
    half_b: Vec2,
) -> Option<(Vec2, f32)> {
    let diff = pos_a - pos_b;
    let overlap_x = half_a.x + half_b.x - diff.x.abs();
    let overlap_y = half_a.y + half_b.y - diff.y.abs();

    if overlap_x > 0.0 && overlap_y > 0.0 {
        if overlap_x < overlap_y {
            let axis = if diff.x > 0.0 { Vec2::X } else { Vec2::NEG_X };
            Some((axis, overlap_x))
        } else {
            let axis = if diff.y > 0.0 { Vec2::Y } else { Vec2::NEG_Y };
            Some((axis, overlap_y))
        }
    } else {
        None
    }
}

fn half_extents(size: Vec2, factor: f32) -> Vec2 {
    size * factor
}

// ---------------------------------------------------------------------------
// Setup
// ---------------------------------------------------------------------------

fn setup(
    mut commands: Commands,
    asset_server: Res<AssetServer>,
    mut atlas_layouts: ResMut<Assets<TextureAtlasLayout>>,
) {
    // Camera centred so (0,0) is bottom-left, matching the original coordinate
    // system.
    commands.spawn((
        Camera2d,
        Transform::from_xyz(WINDOW_WIDTH / 2.0, WINDOW_HEIGHT / 2.0, 999.0),
    ));

    // ---- Sky background ----
    commands.spawn((
        Sprite {
            image: asset_server.load("sky.png"),
            custom_size: Some(Vec2::new(WINDOW_WIDTH, 700.0)),
            ..default()
        },
        Transform::from_xyz(400.0, 300.0, 0.0),
    ));

    // ---- Ground ----
    commands.spawn((
        Sprite {
            image: asset_server.load("ground.png"),
            custom_size: Some(Vec2::new(2000.0, 500.0)),
            ..default()
        },
        Transform::from_xyz(400.0, -200.0, 1.0),
        Ground,
        SpriteSize(Vec2::new(2000.0, 500.0)),
        ColliderExtents(0.42),
    ));

    // ---- Player (Mario) ----
    let layout = TextureAtlasLayout::from_grid(
        UVec2::new(MARIO_TILE_W, MARIO_TILE_H),
        MARIO_COLS,
        MARIO_ROWS,
        None,
        None,
    );
    let layout_handle = atlas_layouts.add(layout);

    commands.spawn((
        Sprite {
            image: asset_server.load("marioBig.png"),
            texture_atlas: Some(TextureAtlas {
                layout: layout_handle,
                index: 0,
            }),
            custom_size: Some(Vec2::new(50.0, 75.0)),
            ..default()
        },
        Transform::from_xyz(10.0, 21.0, 5.0),
        Player {
            gravity: GRAVITY,
            is_grounded: false,
            is_on_platform: false,
            end_game: false,
        },
        Velocity(Vec2::ZERO),
        Impulse(Vec2::ZERO),
        Force(Vec2::ZERO),
        Mass(1.0),
        Drag(PLAYER_DRAG_GROUNDED),
        SpriteSize(Vec2::new(50.0, 75.0)),
        ColliderExtents(0.5),
        AnimState {
            min: 0,
            max: 0,
            idx: 0,
            timer: 0.1,
            start_timer: 0.1,
            facing_right: true,
        },
    ));

    // ---- Fireworks (hidden until win) ----
    for i in 0..8u32 {
        commands.spawn((
            Sprite {
                image: asset_server.load(format!("firework_red{i}.png")),
                custom_size: Some(Vec2::new(40.0, 40.0)),
                ..default()
            },
            Transform::from_xyz(-200.0, -200.0, 8.0),
            Visibility::Hidden,
            Firework,
        ));
    }

    // ---- Win Text ----
    commands.spawn((
        Text2d::new("Good Job!"),
        TextFont {
            font_size: 60.0,
            ..default()
        },
        TextColor(Color::WHITE),
        Transform::from_xyz(400.0, 300.0, 10.0),
        Visibility::Hidden,
        WinText,
    ));

    // ---- Level Banner (shows "Level 1" / "Level 2" briefly) ----
    commands.spawn((
        Text2d::new("Level 1"),
        TextFont {
            font_size: 72.0,
            ..default()
        },
        TextColor(Color::WHITE),
        Transform::from_xyz(400.0, 350.0, 10.0),
        LevelBanner(2.0),
    ));

    // Resources
    commands.insert_resource(PlayerPlatformLink::default());
    commands.insert_resource(CurrentLevel(1));

    // Spawn Level 1 platforms and goal
    spawn_level1(&mut commands, &*asset_server);
}

// ---------------------------------------------------------------------------
// Level 1 – Original layout
// ---------------------------------------------------------------------------

fn spawn_level1(commands: &mut Commands, asset_server: &AssetServer) {
    let wall_tex: Handle<Image> = asset_server.load("wall.png");
    let ground_tex: Handle<Image> = asset_server.load("ground.png");
    let paddle_tex: Handle<Image> = asset_server.load("paddle.png");

    // Goal (potato) – top-right
    commands.spawn((
        Sprite {
            image: asset_server.load("potato.png"),
            custom_size: Some(Vec2::new(29.5, 28.125)),
            ..default()
        },
        Transform::from_xyz(780.0, 530.0, 2.0),
        Goal,
        SpriteSize(Vec2::new(29.5, 28.125)),
        ColliderExtents(0.5),
        LevelEntity,
    ));

    // Static[0]: wall, 150×200 at (500, 40)
    spawn_platform_lv(
        commands,
        wall_tex.clone(),
        Vec2::new(150.0, 200.0),
        Vec2::new(500.0, 40.0),
        PlatformKind::Static,
    );
    // Static[1]: wall, 150×50 at (0, 350)
    spawn_platform_lv(
        commands,
        wall_tex.clone(),
        Vec2::new(150.0, 50.0),
        Vec2::new(0.0, 350.0),
        PlatformKind::Static,
    );
    // Static[2]: ground texture, 150×50 at (800, 500)
    spawn_platform_lv(
        commands,
        ground_tex,
        Vec2::new(150.0, 50.0),
        Vec2::new(800.0, 500.0),
        PlatformKind::Static,
    );

    // Horizontal Moving Platform
    spawn_platform_lv(
        commands,
        paddle_tex.clone(),
        Vec2::new(88.0, 18.0),
        Vec2::new(600.0, 250.0),
        PlatformKind::Horizontal {
            min_x: 600.0,
            max_x: 750.0,
            speed: PLATFORM_SPEED,
            moving_right: true,
        },
    );

    // UpRight Moving Platform
    spawn_platform_lv(
        commands,
        paddle_tex.clone(),
        Vec2::new(88.0, 18.0),
        Vec2::new(25.0, 30.0),
        PlatformKind::UpRight {
            min_x: 110.0,
            max_x: 275.0,
            speed: PLATFORM_SPEED,
            moving_right: true,
            moving_up: true,
        },
    );

    // UpLeft Moving Platform
    spawn_platform_lv(
        commands,
        paddle_tex.clone(),
        Vec2::new(88.0, 18.0),
        Vec2::new(520.0, 280.0),
        PlatformKind::UpLeft {
            min_x: 150.0,
            max_x: 500.0,
            speed: PLATFORM_SPEED,
            moving_right: false,
            moving_up: true,
        },
    );

    // Vertical Moving Platforms
    spawn_platform_lv(
        commands,
        paddle_tex.clone(),
        Vec2::new(88.0, 18.0),
        Vec2::new(500.0, 180.0),
        PlatformKind::Vertical {
            min_y: 180.0,
            max_y: 225.0,
            speed: PLATFORM_SPEED,
            moving_up: true,
        },
    );
    spawn_platform_lv(
        commands,
        paddle_tex.clone(),
        Vec2::new(44.0, 18.0),
        Vec2::new(620.0, 20.0),
        PlatformKind::Vertical {
            min_y: 20.0,
            max_y: 100.0,
            speed: PLATFORM_SPEED,
            moving_up: true,
        },
    );
    spawn_platform_lv(
        commands,
        paddle_tex.clone(),
        Vec2::new(50.0, 18.0),
        Vec2::new(90.0, 420.0),
        PlatformKind::Vertical {
            min_y: 400.0,
            max_y: 460.0,
            speed: PLATFORM_SPEED,
            moving_up: true,
        },
    );

    // Multi-Directional Moving Platform
    spawn_platform_lv(
        commands,
        paddle_tex,
        Vec2::new(88.0, 18.0),
        Vec2::new(125.0, 460.0),
        PlatformKind::MultiDir {
            min_x: 200.0,
            mid_x: 250.0,
            max_x: 650.0,
            speed: PLATFORM_SPEED,
            moving_right: true,
            moving_up: true,
            moving_left: false,
            moving_down: false,
        },
    );
}

// ---------------------------------------------------------------------------
// Level 2 – "The Ascent" – harder layout, faster platforms
// ---------------------------------------------------------------------------

fn spawn_level2(commands: &mut Commands, asset_server: &AssetServer) {
    let wall_tex: Handle<Image> = asset_server.load("wall.png");
    let ground_tex: Handle<Image> = asset_server.load("ground.png");
    let paddle_tex: Handle<Image> = asset_server.load("paddle.png");

    // Goal (potato) – top-left, reached by climbing
    commands.spawn((
        Sprite {
            image: asset_server.load("potato.png"),
            custom_size: Some(Vec2::new(29.5, 28.125)),
            ..default()
        },
        Transform::from_xyz(50.0, 560.0, 2.0),
        Goal,
        SpriteSize(Vec2::new(29.5, 28.125)),
        ColliderExtents(0.5),
        LevelEntity,
    ));

    // ---- Static Platforms: stepping-stone path ----

    // Low ledge on the right – first jump target from ground
    spawn_platform_lv(
        commands,
        wall_tex.clone(),
        Vec2::new(120.0, 30.0),
        Vec2::new(700.0, 80.0),
        PlatformKind::Static,
    );

    // Mid wall block – rest point
    spawn_platform_lv(
        commands,
        wall_tex.clone(),
        Vec2::new(100.0, 120.0),
        Vec2::new(200.0, 160.0),
        PlatformKind::Static,
    );

    // Upper-right ledge
    spawn_platform_lv(
        commands,
        ground_tex.clone(),
        Vec2::new(100.0, 25.0),
        Vec2::new(650.0, 320.0),
        PlatformKind::Static,
    );

    // Small step near top-left (just below goal)
    spawn_platform_lv(
        commands,
        wall_tex.clone(),
        Vec2::new(80.0, 25.0),
        Vec2::new(100.0, 520.0),
        PlatformKind::Static,
    );

    // Narrow pillar obstacle in the middle
    spawn_platform_lv(
        commands,
        wall_tex.clone(),
        Vec2::new(40.0, 180.0),
        Vec2::new(400.0, 120.0),
        PlatformKind::Static,
    );

    // ---- Moving Platforms (faster speed for Level 2) ----

    // Lower horizontal ferry – carries player from right to mid wall
    spawn_platform_lv(
        commands,
        paddle_tex.clone(),
        Vec2::new(88.0, 18.0),
        Vec2::new(500.0, 120.0),
        PlatformKind::Horizontal {
            min_x: 300.0,
            max_x: 650.0,
            speed: PLATFORM_SPEED_L2,
            moving_right: true,
        },
    );

    // Upper horizontal ferry
    spawn_platform_lv(
        commands,
        paddle_tex.clone(),
        Vec2::new(70.0, 18.0),
        Vec2::new(500.0, 420.0),
        PlatformKind::Horizontal {
            min_x: 350.0,
            max_x: 700.0,
            speed: PLATFORM_SPEED_L2,
            moving_right: false,
        },
    );

    // Left elevator – rides up from mid to upper area
    spawn_platform_lv(
        commands,
        paddle_tex.clone(),
        Vec2::new(60.0, 18.0),
        Vec2::new(100.0, 250.0),
        PlatformKind::Vertical {
            min_y: 250.0,
            max_y: 450.0,
            speed: PLATFORM_SPEED_L2,
            moving_up: true,
        },
    );

    // Right elevator – fast, short stroke
    spawn_platform_lv(
        commands,
        paddle_tex.clone(),
        Vec2::new(50.0, 18.0),
        Vec2::new(750.0, 150.0),
        PlatformKind::Vertical {
            min_y: 150.0,
            max_y: 300.0,
            speed: PLATFORM_SPEED_L2,
            moving_up: true,
        },
    );

    // Diagonal ramp up-right – lifts from lower-left area
    spawn_platform_lv(
        commands,
        paddle_tex.clone(),
        Vec2::new(88.0, 18.0),
        Vec2::new(250.0, 60.0),
        PlatformKind::UpRight {
            min_x: 250.0,
            max_x: 500.0,
            speed: PLATFORM_SPEED_L2,
            moving_right: true,
            moving_up: true,
        },
    );

    // Diagonal ramp up-left – descends from upper-right
    spawn_platform_lv(
        commands,
        paddle_tex.clone(),
        Vec2::new(70.0, 18.0),
        Vec2::new(600.0, 350.0),
        PlatformKind::UpLeft {
            min_x: 200.0,
            max_x: 600.0,
            speed: PLATFORM_SPEED_L2,
            moving_right: false,
            moving_up: true,
        },
    );

    // Multi-directional platform – complex path through upper area
    spawn_platform_lv(
        commands,
        paddle_tex.clone(),
        Vec2::new(88.0, 18.0),
        Vec2::new(300.0, 480.0),
        PlatformKind::MultiDir {
            min_x: 200.0,
            mid_x: 350.0,
            max_x: 600.0,
            speed: PLATFORM_SPEED_L2,
            moving_right: true,
            moving_up: true,
            moving_left: false,
            moving_down: false,
        },
    );

    // Extra small vertical platform – tricky timing
    spawn_platform_lv(
        commands,
        paddle_tex,
        Vec2::new(44.0, 18.0),
        Vec2::new(350.0, 350.0),
        PlatformKind::Vertical {
            min_y: 300.0,
            max_y: 420.0,
            speed: PLATFORM_SPEED_L2,
            moving_up: true,
        },
    );
}

/// Spawn a platform tagged with `LevelEntity` so it is despawned between levels.
fn spawn_platform_lv(
    commands: &mut Commands,
    texture: Handle<Image>,
    size: Vec2,
    pos: Vec2,
    kind: PlatformKind,
) {
    commands.spawn((
        Sprite {
            image: texture,
            custom_size: Some(size),
            ..default()
        },
        Transform::from_xyz(pos.x, pos.y, 3.0),
        Platform,
        kind,
        SpriteSize(size),
        ColliderExtents(0.5),
        Velocity(Vec2::ZERO),
        Mass(100.0),
        LevelEntity,
    ));
}

/// Brief text banner showing the current level name.
#[derive(Component)]
struct LevelBanner(f32); // remaining seconds to display

// ---------------------------------------------------------------------------
// Level helpers
// ---------------------------------------------------------------------------

fn spawn_goal_at(commands: &mut Commands, asset_server: &AssetServer, x: f32, y: f32) {
    commands.spawn((
        Sprite {
            image: asset_server.load("potato.png"),
            custom_size: Some(Vec2::new(29.5, 28.125)),
            ..default()
        },
        Transform::from_xyz(x, y, 2.0),
        Goal,
        SpriteSize(Vec2::new(29.5, 28.125)),
        ColliderExtents(0.5),
        LevelEntity,
    ));
}

/// Shorthand: static platform
fn sp_static(c: &mut Commands, tex: Handle<Image>, w: f32, h: f32, x: f32, y: f32) {
    spawn_platform_lv(c, tex, Vec2::new(w, h), Vec2::new(x, y), PlatformKind::Static);
}

/// Shorthand: horizontal moving platform
fn sp_horiz(c: &mut Commands, tex: Handle<Image>, w: f32, h: f32, x: f32, y: f32, min_x: f32, max_x: f32, speed: f32) {
    spawn_platform_lv(c, tex, Vec2::new(w, h), Vec2::new(x, y), PlatformKind::Horizontal {
        min_x, max_x, speed, moving_right: true,
    });
}

/// Shorthand: vertical moving platform
fn sp_vert(c: &mut Commands, tex: Handle<Image>, w: f32, h: f32, x: f32, y: f32, min_y: f32, max_y: f32, speed: f32) {
    spawn_platform_lv(c, tex, Vec2::new(w, h), Vec2::new(x, y), PlatformKind::Vertical {
        min_y, max_y, speed, moving_up: true,
    });
}

/// Shorthand: diagonal up-right
fn sp_ur(c: &mut Commands, tex: Handle<Image>, w: f32, h: f32, x: f32, y: f32, min_x: f32, max_x: f32, speed: f32) {
    spawn_platform_lv(c, tex, Vec2::new(w, h), Vec2::new(x, y), PlatformKind::UpRight {
        min_x, max_x, speed, moving_right: true, moving_up: true,
    });
}

/// Shorthand: diagonal up-left
fn sp_ul(c: &mut Commands, tex: Handle<Image>, w: f32, h: f32, x: f32, y: f32, min_x: f32, max_x: f32, speed: f32) {
    spawn_platform_lv(c, tex, Vec2::new(w, h), Vec2::new(x, y), PlatformKind::UpLeft {
        min_x, max_x, speed, moving_right: false, moving_up: true,
    });
}

/// Shorthand: multi-directional
fn sp_multi(c: &mut Commands, tex: Handle<Image>, w: f32, h: f32, x: f32, y: f32, min_x: f32, mid_x: f32, max_x: f32, speed: f32) {
    spawn_platform_lv(c, tex, Vec2::new(w, h), Vec2::new(x, y), PlatformKind::MultiDir {
        min_x, mid_x, max_x, speed, moving_right: true, moving_up: true, moving_left: false, moving_down: false,
    });
}

fn tex(asset_server: &AssetServer, name: &str) -> Handle<Image> {
    asset_server.load(format!("{name}.png"))
}

// ---------------------------------------------------------------------------
// Level dispatcher
// ---------------------------------------------------------------------------

fn spawn_level(commands: &mut Commands, asset_server: &AssetServer, level: u32) {
    match level {
        1  => spawn_level1(commands, asset_server),
        2  => spawn_level2(commands, asset_server),
        _  => spawn_level_n(commands, asset_server, level),
    }
}

fn spawn_level_n(c: &mut Commands, a: &AssetServer, level: u32) {
    let wall = tex(a, "wall");
    let gnd = tex(a, "ground");
    let pad = tex(a, "paddle");

    match level {
        // ==================================================================
        // Level 3 – "Stepping Stones" – small static hops across the screen
        // ==================================================================
        3 => {
            spawn_goal_at(c, a, 750.0, 300.0);
            sp_static(c, wall.clone(), 80.0, 25.0, 150.0, 80.0);
            sp_static(c, wall.clone(), 70.0, 25.0, 300.0, 140.0);
            sp_static(c, wall.clone(), 70.0, 25.0, 450.0, 200.0);
            sp_static(c, wall.clone(), 80.0, 25.0, 600.0, 260.0);
            sp_static(c, gnd.clone(), 90.0, 25.0, 740.0, 280.0);
            sp_horiz(c, pad, 70.0, 18.0, 220.0, 110.0, 180.0, 350.0, 1.5);
        }

        // ==================================================================
        // Level 4 – "Elevator Express" – vertical elevators staggered L-C-R
        // ==================================================================
        4 => {
            spawn_goal_at(c, a, 400.0, 560.0);
            sp_static(c, wall.clone(), 100.0, 25.0, 100.0, 80.0);
            sp_static(c, wall.clone(), 80.0, 25.0, 400.0, 280.0);
            sp_static(c, gnd.clone(), 100.0, 25.0, 400.0, 530.0);
            sp_vert(c, pad.clone(), 70.0, 18.0, 100.0, 100.0, 80.0, 260.0, 1.5);
            sp_vert(c, pad.clone(), 70.0, 18.0, 700.0, 180.0, 150.0, 380.0, 1.75);
            sp_vert(c, pad, 60.0, 18.0, 400.0, 340.0, 310.0, 500.0, 1.5);
            sp_static(c, wall, 80.0, 25.0, 700.0, 180.0);
        }

        // ==================================================================
        // Level 5 – "Zigzag" – diagonal platforms forming a zigzag path up
        // ==================================================================
        5 => {
            spawn_goal_at(c, a, 50.0, 500.0);
            sp_static(c, wall.clone(), 80.0, 25.0, 700.0, 70.0);
            sp_static(c, wall.clone(), 60.0, 25.0, 100.0, 460.0);
            sp_ur(c, pad.clone(), 88.0, 18.0, 650.0, 80.0, 500.0, 700.0, 1.75);
            sp_ul(c, pad.clone(), 88.0, 18.0, 300.0, 200.0, 100.0, 400.0, 1.75);
            sp_ur(c, pad.clone(), 88.0, 18.0, 200.0, 280.0, 200.0, 500.0, 1.75);
            sp_ul(c, pad, 80.0, 18.0, 500.0, 380.0, 150.0, 550.0, 1.75);
        }

        // ==================================================================
        // Level 6 – "The Corridor" – wall barriers with ferries between
        // ==================================================================
        6 => {
            spawn_goal_at(c, a, 750.0, 530.0);
            // Tall wall barriers
            sp_static(c, wall.clone(), 40.0, 300.0, 250.0, 150.0);
            sp_static(c, wall.clone(), 40.0, 300.0, 500.0, 250.0);
            sp_static(c, wall.clone(), 40.0, 200.0, 700.0, 350.0);
            // Ferries between corridors
            sp_horiz(c, pad.clone(), 88.0, 18.0, 120.0, 130.0, 50.0, 230.0, 1.75);
            sp_vert(c, pad.clone(), 60.0, 18.0, 370.0, 200.0, 100.0, 350.0, 2.0);
            sp_horiz(c, pad.clone(), 80.0, 18.0, 580.0, 380.0, 520.0, 680.0, 2.0);
            sp_vert(c, pad, 50.0, 18.0, 600.0, 400.0, 380.0, 520.0, 1.75);
            // Rest ledge
            sp_static(c, gnd, 100.0, 25.0, 750.0, 500.0);
        }

        // ==================================================================
        // Level 7 – "Speed Demons" – fast platforms, wide gaps
        // ==================================================================
        7 => {
            spawn_goal_at(c, a, 780.0, 400.0);
            sp_static(c, wall.clone(), 80.0, 25.0, 780.0, 370.0);
            sp_horiz(c, pad.clone(), 80.0, 18.0, 200.0, 100.0, 100.0, 400.0, 2.5);
            sp_horiz(c, pad.clone(), 70.0, 18.0, 600.0, 200.0, 450.0, 750.0, 2.5);
            sp_horiz(c, pad.clone(), 75.0, 18.0, 300.0, 300.0, 150.0, 550.0, 2.5);
            sp_vert(c, pad, 60.0, 18.0, 700.0, 300.0, 280.0, 380.0, 2.5);
        }

        // ==================================================================
        // Level 8 – "Stairway" – static staircase with moving obstacles
        // ==================================================================
        8 => {
            spawn_goal_at(c, a, 750.0, 530.0);
            // Staircase steps
            sp_static(c, wall.clone(), 90.0, 20.0, 100.0, 80.0);
            sp_static(c, wall.clone(), 90.0, 20.0, 250.0, 160.0);
            sp_static(c, wall.clone(), 90.0, 20.0, 400.0, 240.0);
            sp_static(c, wall.clone(), 90.0, 20.0, 550.0, 320.0);
            sp_static(c, wall.clone(), 90.0, 20.0, 700.0, 400.0);
            sp_static(c, gnd, 100.0, 25.0, 750.0, 500.0);
            // Moving obstacles between steps
            sp_horiz(c, pad.clone(), 60.0, 18.0, 180.0, 140.0, 120.0, 300.0, 2.0);
            sp_horiz(c, pad.clone(), 60.0, 18.0, 330.0, 220.0, 280.0, 460.0, 2.0);
            sp_vert(c, pad, 50.0, 18.0, 630.0, 350.0, 330.0, 400.0, 2.0);
        }

        // ==================================================================
        // Level 9 – "Double Decker" – two layers of horizontal ferries
        // ==================================================================
        9 => {
            spawn_goal_at(c, a, 50.0, 500.0);
            // Lower layer ferries
            sp_horiz(c, pad.clone(), 88.0, 18.0, 200.0, 120.0, 100.0, 500.0, 1.75);
            sp_horiz(c, pad.clone(), 80.0, 18.0, 600.0, 120.0, 450.0, 750.0, 2.0);
            // Upper layer ferries
            sp_horiz(c, pad.clone(), 80.0, 18.0, 500.0, 350.0, 300.0, 700.0, 2.0);
            sp_horiz(c, pad.clone(), 70.0, 18.0, 150.0, 350.0, 50.0, 350.0, 1.75);
            // Vertical connectors
            sp_vert(c, pad.clone(), 50.0, 18.0, 750.0, 150.0, 120.0, 340.0, 2.0);
            sp_vert(c, pad, 50.0, 18.0, 50.0, 360.0, 350.0, 480.0, 2.0);
            // Rest ledge near goal
            sp_static(c, wall, 80.0, 25.0, 50.0, 470.0);
        }

        // ==================================================================
        // Level 10 – "Around the Tower" – central pillar, platforms wrap
        // ==================================================================
        10 => {
            spawn_goal_at(c, a, 400.0, 570.0);
            // Central tower
            sp_static(c, wall.clone(), 80.0, 350.0, 400.0, 200.0);
            // Platforms wrapping around
            sp_static(c, gnd.clone(), 120.0, 25.0, 150.0, 80.0);
            sp_static(c, gnd, 120.0, 25.0, 650.0, 80.0);
            sp_horiz(c, pad.clone(), 80.0, 18.0, 200.0, 200.0, 80.0, 340.0, 2.0);
            sp_horiz(c, pad.clone(), 80.0, 18.0, 600.0, 300.0, 460.0, 750.0, 2.0);
            sp_vert(c, pad.clone(), 60.0, 18.0, 80.0, 250.0, 200.0, 400.0, 2.0);
            sp_vert(c, pad.clone(), 60.0, 18.0, 720.0, 350.0, 300.0, 500.0, 2.0);
            sp_horiz(c, pad, 100.0, 18.0, 400.0, 520.0, 200.0, 600.0, 1.5);
            // Ledge just below goal
            sp_static(c, wall, 80.0, 20.0, 400.0, 545.0);
        }

        // ==================================================================
        // Level 11 – "Chaos Theory" – multi-dir platforms dominate
        // ==================================================================
        11 => {
            spawn_goal_at(c, a, 750.0, 350.0);
            sp_static(c, wall.clone(), 80.0, 25.0, 100.0, 80.0);
            sp_static(c, wall, 80.0, 25.0, 750.0, 320.0);
            sp_multi(c, pad.clone(), 88.0, 18.0, 200.0, 100.0, 150.0, 300.0, 500.0, 2.0);
            sp_multi(c, pad.clone(), 80.0, 18.0, 400.0, 250.0, 250.0, 400.0, 650.0, 2.0);
            sp_multi(c, pad.clone(), 75.0, 18.0, 300.0, 380.0, 200.0, 350.0, 600.0, 2.25);
            sp_vert(c, pad, 50.0, 18.0, 700.0, 200.0, 150.0, 330.0, 2.0);
        }

        // ==================================================================
        // Level 12 – "Minimalist" – few tiny platforms, precision jumps
        // ==================================================================
        12 => {
            spawn_goal_at(c, a, 50.0, 560.0);
            sp_static(c, wall.clone(), 50.0, 20.0, 200.0, 90.0);
            sp_static(c, wall.clone(), 45.0, 20.0, 400.0, 180.0);
            sp_static(c, wall.clone(), 45.0, 20.0, 600.0, 270.0);
            sp_static(c, wall, 50.0, 20.0, 400.0, 370.0);
            sp_vert(c, pad.clone(), 44.0, 18.0, 200.0, 370.0, 350.0, 520.0, 2.0);
            sp_static(c, gnd, 60.0, 20.0, 50.0, 530.0);
            sp_horiz(c, pad, 50.0, 18.0, 600.0, 460.0, 400.0, 700.0, 2.25);
        }

        // ==================================================================
        // Level 13 – "Crossroads" – diagonals crossing each other
        // ==================================================================
        13 => {
            spawn_goal_at(c, a, 400.0, 570.0);
            sp_static(c, wall.clone(), 80.0, 25.0, 700.0, 70.0);
            sp_static(c, wall, 80.0, 20.0, 400.0, 540.0);
            sp_ur(c, pad.clone(), 88.0, 18.0, 600.0, 80.0, 400.0, 700.0, 2.0);
            sp_ul(c, pad.clone(), 88.0, 18.0, 300.0, 150.0, 100.0, 450.0, 2.0);
            sp_ur(c, pad.clone(), 80.0, 18.0, 150.0, 250.0, 100.0, 500.0, 2.25);
            sp_ul(c, pad.clone(), 80.0, 18.0, 600.0, 350.0, 200.0, 650.0, 2.25);
            sp_vert(c, pad, 50.0, 18.0, 300.0, 400.0, 380.0, 530.0, 2.0);
        }

        // ==================================================================
        // Level 14 – "Traffic Jam" – many horizontals, different speeds
        // ==================================================================
        14 => {
            spawn_goal_at(c, a, 780.0, 500.0);
            sp_static(c, gnd, 90.0, 25.0, 780.0, 470.0);
            sp_horiz(c, pad.clone(), 88.0, 18.0, 100.0, 80.0, 50.0, 400.0, 1.5);
            sp_horiz(c, pad.clone(), 70.0, 18.0, 600.0, 150.0, 400.0, 750.0, 2.5);
            sp_horiz(c, pad.clone(), 80.0, 18.0, 200.0, 220.0, 50.0, 500.0, 2.0);
            sp_horiz(c, pad.clone(), 65.0, 18.0, 550.0, 300.0, 350.0, 750.0, 2.75);
            sp_horiz(c, pad.clone(), 75.0, 18.0, 150.0, 380.0, 50.0, 450.0, 2.25);
            sp_vert(c, pad, 50.0, 18.0, 700.0, 380.0, 370.0, 470.0, 2.5);
        }

        // ==================================================================
        // Level 15 – "The Chimney" – narrow vertical channel
        // ==================================================================
        15 => {
            spawn_goal_at(c, a, 400.0, 575.0);
            // Chimney walls
            sp_static(c, wall.clone(), 40.0, 600.0, 280.0, 300.0);
            sp_static(c, wall.clone(), 40.0, 600.0, 520.0, 300.0);
            // Alternating platforms inside chimney
            sp_static(c, gnd.clone(), 80.0, 20.0, 350.0, 80.0);
            sp_static(c, gnd.clone(), 80.0, 20.0, 450.0, 180.0);
            sp_static(c, gnd.clone(), 80.0, 20.0, 350.0, 280.0);
            sp_static(c, gnd, 80.0, 20.0, 450.0, 380.0);
            sp_vert(c, pad.clone(), 60.0, 18.0, 400.0, 420.0, 400.0, 550.0, 2.5);
            // Entry platform outside chimney
            sp_static(c, wall, 100.0, 25.0, 150.0, 60.0);
            sp_horiz(c, pad, 70.0, 18.0, 200.0, 60.0, 150.0, 300.0, 1.5);
        }

        // ==================================================================
        // Level 16 – "Mirror Mirror" – symmetric layout, two paths up
        // ==================================================================
        16 => {
            spawn_goal_at(c, a, 400.0, 575.0);
            // Symmetric static ledges
            sp_static(c, wall.clone(), 80.0, 25.0, 150.0, 100.0);
            sp_static(c, wall.clone(), 80.0, 25.0, 650.0, 100.0);
            sp_static(c, wall.clone(), 70.0, 25.0, 250.0, 250.0);
            sp_static(c, wall.clone(), 70.0, 25.0, 550.0, 250.0);
            sp_static(c, wall.clone(), 80.0, 25.0, 150.0, 400.0);
            sp_static(c, wall, 80.0, 25.0, 650.0, 400.0);
            sp_static(c, gnd, 100.0, 25.0, 400.0, 545.0);
            // Symmetric vertical elevators
            sp_vert(c, pad.clone(), 50.0, 18.0, 150.0, 130.0, 100.0, 380.0, 2.25);
            sp_vert(c, pad.clone(), 50.0, 18.0, 650.0, 130.0, 100.0, 380.0, 2.25);
            // Center connector
            sp_horiz(c, pad, 80.0, 18.0, 400.0, 460.0, 250.0, 550.0, 2.0);
        }

        // ==================================================================
        // Level 17 – "The Labyrinth" – dense walls creating a maze
        // ==================================================================
        17 => {
            spawn_goal_at(c, a, 700.0, 80.0);
            // Maze walls
            sp_static(c, wall.clone(), 30.0, 150.0, 200.0, 80.0);
            sp_static(c, wall.clone(), 200.0, 30.0, 350.0, 150.0);
            sp_static(c, wall.clone(), 30.0, 200.0, 500.0, 200.0);
            sp_static(c, wall.clone(), 200.0, 30.0, 350.0, 300.0);
            sp_static(c, wall.clone(), 30.0, 150.0, 200.0, 380.0);
            sp_static(c, wall, 150.0, 30.0, 550.0, 400.0);
            // Moving platforms to navigate maze
            sp_horiz(c, pad.clone(), 60.0, 18.0, 100.0, 100.0, 50.0, 180.0, 2.0);
            sp_vert(c, pad.clone(), 50.0, 18.0, 300.0, 80.0, 50.0, 140.0, 2.5);
            sp_horiz(c, pad.clone(), 60.0, 18.0, 400.0, 230.0, 340.0, 480.0, 2.25);
            sp_vert(c, pad.clone(), 50.0, 18.0, 600.0, 300.0, 160.0, 390.0, 2.5);
            sp_horiz(c, pad, 70.0, 18.0, 500.0, 450.0, 400.0, 700.0, 2.0);
            sp_static(c, gnd, 80.0, 20.0, 700.0, 55.0);
        }

        // ==================================================================
        // Level 18 – "Turbo Mode" – everything moves at 3.0 speed
        // ==================================================================
        18 => {
            spawn_goal_at(c, a, 50.0, 560.0);
            sp_static(c, wall.clone(), 60.0, 20.0, 50.0, 530.0);
            sp_static(c, wall, 80.0, 25.0, 700.0, 70.0);
            sp_horiz(c, pad.clone(), 80.0, 18.0, 500.0, 80.0, 350.0, 700.0, 3.0);
            sp_vert(c, pad.clone(), 55.0, 18.0, 200.0, 100.0, 80.0, 250.0, 3.0);
            sp_ur(c, pad.clone(), 80.0, 18.0, 400.0, 200.0, 300.0, 600.0, 3.0);
            sp_horiz(c, pad.clone(), 70.0, 18.0, 200.0, 350.0, 50.0, 400.0, 3.0);
            sp_ul(c, pad.clone(), 75.0, 18.0, 500.0, 400.0, 150.0, 550.0, 3.0);
            sp_vert(c, pad, 50.0, 18.0, 100.0, 430.0, 400.0, 520.0, 3.0);
        }

        // ==================================================================
        // Level 19 – "Needle Threading" – tiny platforms, exact jumps
        // ==================================================================
        19 => {
            spawn_goal_at(c, a, 780.0, 560.0);
            sp_static(c, wall.clone(), 40.0, 15.0, 180.0, 80.0);
            sp_static(c, wall.clone(), 35.0, 15.0, 350.0, 150.0);
            sp_static(c, wall.clone(), 35.0, 15.0, 520.0, 230.0);
            sp_static(c, wall.clone(), 40.0, 15.0, 350.0, 310.0);
            sp_static(c, wall.clone(), 35.0, 15.0, 180.0, 390.0);
            sp_static(c, wall, 40.0, 15.0, 400.0, 470.0);
            sp_vert(c, pad.clone(), 38.0, 18.0, 600.0, 320.0, 280.0, 430.0, 3.0);
            sp_horiz(c, pad.clone(), 40.0, 18.0, 650.0, 500.0, 550.0, 780.0, 3.0);
            sp_static(c, gnd, 50.0, 15.0, 780.0, 535.0);
            sp_vert(c, pad, 35.0, 18.0, 250.0, 200.0, 150.0, 310.0, 2.75);
        }

        // ==================================================================
        // Level 20 – "The Gauntlet" – ultimate challenge, all types combined
        // ==================================================================
        20 => {
            spawn_goal_at(c, a, 400.0, 580.0);
            // Bottom tier
            sp_static(c, wall.clone(), 60.0, 20.0, 700.0, 60.0);
            sp_horiz(c, pad.clone(), 55.0, 18.0, 400.0, 60.0, 250.0, 650.0, 3.0);
            // Tier 2
            sp_ur(c, pad.clone(), 60.0, 18.0, 200.0, 80.0, 100.0, 400.0, 3.0);
            sp_static(c, wall.clone(), 40.0, 15.0, 600.0, 180.0);
            // Tier 3
            sp_ul(c, pad.clone(), 55.0, 18.0, 500.0, 220.0, 200.0, 600.0, 3.25);
            sp_vert(c, pad.clone(), 40.0, 18.0, 100.0, 200.0, 180.0, 330.0, 3.0);
            // Tier 4
            sp_horiz(c, pad.clone(), 50.0, 18.0, 300.0, 330.0, 150.0, 500.0, 3.25);
            sp_static(c, wall.clone(), 35.0, 15.0, 700.0, 320.0);
            // Tier 5
            sp_multi(c, pad.clone(), 55.0, 18.0, 500.0, 400.0, 300.0, 450.0, 700.0, 3.0);
            sp_vert(c, pad.clone(), 38.0, 18.0, 150.0, 380.0, 350.0, 480.0, 3.25);
            // Summit
            sp_horiz(c, pad, 45.0, 18.0, 400.0, 530.0, 250.0, 550.0, 3.5);
            sp_static(c, gnd, 60.0, 20.0, 400.0, 555.0);
        }

        _ => {
            // Fallback: re-use Level 1
            spawn_level1(c, a);
        }
    }
}

// ---------------------------------------------------------------------------
// Player Input System
// ---------------------------------------------------------------------------

fn player_input(
    keys: Res<ButtonInput<KeyCode>>,
    mut query: Query<(
        &mut Player,
        &mut Velocity,
        &mut Impulse,
        &mut Force,
        &mut Drag,
        &SpriteSize,
    )>,
) {
    for (player, mut vel, mut imp, mut force, mut drag, _size) in &mut query {
        if player.end_game {
            return;
        }

        // Horizontal movement – force = dimension.x * PLAYER_MOVE_FORCE
        // Player dimension is (50, 75); force matches original: 50 * 5 = 250
        let move_force = 50.0 * PLAYER_MOVE_FORCE;
        if keys.pressed(KeyCode::KeyD) {
            force.0.x = move_force;
        }
        if keys.pressed(KeyCode::KeyA) {
            force.0.x = -move_force;
        }
        // Instant stop when neither key pressed (matches original)
        if !keys.pressed(KeyCode::KeyA) && !keys.pressed(KeyCode::KeyD) {
            vel.0.x = 0.0;
        }

        // Jump
        if keys.pressed(KeyCode::Space) && player.is_grounded {
            imp.0.y += player.gravity.abs() * JUMP_MULTIPLIER;
        }

        // Grounded state
        if player.is_grounded {
            vel.0.y = 0.0;
            drag.0 = PLAYER_DRAG_GROUNDED;
        } else {
            drag.0 = 0.0;
            // Frame-rate-dependent gravity (matches original behavior)
            vel.0.y += player.gravity;
        }
    }
}

// ---------------------------------------------------------------------------
// Platform Movement System
// ---------------------------------------------------------------------------

fn platform_movement(mut query: Query<(&mut PlatformKind, &mut Velocity, &SpriteSize, &Transform)>) {
    for (mut kind, mut vel, size, tf) in &mut query {
        let dim_x = size.0.x;
        let dim_y = size.0.y;
        let px = tf.translation.x;
        let py = tf.translation.y;

        match kind.as_mut() {
            PlatformKind::Static => {}

            PlatformKind::Horizontal {
                min_x,
                max_x,
                speed,
                moving_right,
            } => {
                if *moving_right {
                    vel.0.x = dim_x * *speed;
                    if px > *max_x {
                        *moving_right = false;
                    }
                } else {
                    vel.0.x = -(dim_x * *speed);
                    if px < *min_x {
                        *moving_right = true;
                    }
                }
            }

            PlatformKind::Vertical {
                min_y,
                max_y,
                speed,
                moving_up,
            } => {
                if *moving_up {
                    vel.0.y = dim_y * *speed;
                    if py > *max_y {
                        *moving_up = false;
                    }
                } else {
                    vel.0.y = -(dim_y * *speed);
                    if py <= *min_y {
                        *moving_up = true;
                    }
                }
            }

            PlatformKind::UpRight {
                min_x,
                max_x,
                speed,
                moving_right,
                moving_up,
            } => {
                if *moving_right {
                    vel.0.x = dim_x * *speed;
                    if px > *max_x {
                        *moving_right = false;
                        *moving_up = false;
                    }
                } else {
                    vel.0.x = -(dim_x * *speed);
                    if px < *min_x {
                        *moving_right = true;
                        *moving_up = true;
                    }
                }
                if *moving_up {
                    vel.0.y = dim_y * *speed;
                } else {
                    vel.0.y = -(dim_y * *speed);
                }
            }

            PlatformKind::UpLeft {
                min_x,
                max_x,
                speed,
                moving_right,
                moving_up,
            } => {
                if *moving_right {
                    vel.0.x = dim_x * *speed;
                    if px > *max_x {
                        *moving_right = false;
                        *moving_up = true;
                    }
                } else {
                    vel.0.x = -(dim_x * *speed);
                    if px < *min_x {
                        *moving_right = true;
                        *moving_up = false;
                    }
                }
                if *moving_up {
                    vel.0.y = dim_y * *speed;
                } else {
                    vel.0.y = -(dim_y * *speed);
                }
            }

            PlatformKind::MultiDir {
                min_x,
                mid_x,
                max_x,
                speed,
                moving_right,
                moving_up,
                moving_left,
                moving_down: _,
            } => {
                if *moving_right {
                    vel.0.x = dim_x * *speed;
                    if *moving_up {
                        vel.0.y = dim_y * *speed;
                    }
                    if px >= 400.0 {
                        *moving_up = false;
                        vel.0.y = 0.0;
                    }
                    if px > *max_x {
                        *moving_right = false;
                        *moving_left = true;
                    }
                }
                if *moving_left {
                    vel.0.x = -(dim_x * *speed);
                    if px <= *mid_x + 150.0 {
                        vel.0.y = -(dim_y * *speed);
                    }
                    if px <= *min_x {
                        *moving_left = false;
                        *moving_right = true;
                        *moving_up = true;
                    }
                }
            }
        }
    }
}

// ---------------------------------------------------------------------------
// Physics Integration System
// ---------------------------------------------------------------------------

fn physics_integration(
    time: Res<Time>,
    link: Res<PlayerPlatformLink>,
    mut player_q: Query<
        (&mut Transform, &mut Velocity, &mut Force, &mut Impulse, &Mass, &Drag, &Player),
        Without<Platform>,
    >,
    mut plat_q: Query<(&mut Transform, &Velocity, &Mass), (With<Platform>, Without<Player>)>,
) {
    let dt = time.delta_secs().min(1.0);

    // Integrate player
    for (mut tf, mut vel, mut force, mut imp, mass, drag, _player) in &mut player_q {
        let acc = force.0 / mass.0;
        vel.0 += acc * dt + imp.0 / mass.0;

        // Apply platform velocity (parenting substitute)
        let plat_offset = link.platform_velocity * dt;

        tf.translation.x += vel.0.x * dt + plat_offset.x;
        tf.translation.y += vel.0.y * dt + plat_offset.y;

        imp.0 = Vec2::ZERO;
        force.0 = -vel.0 * drag.0;
    }

    // Integrate platforms
    for (mut tf, vel, _mass) in &mut plat_q {
        tf.translation.x += vel.0.x * dt;
        tf.translation.y += vel.0.y * dt;
    }
}

// ---------------------------------------------------------------------------
// Collision System
// ---------------------------------------------------------------------------

fn collision_system(
    mut player_q: Query<
        (
            &mut Transform,
            &mut Velocity,
            &mut Player,
            &SpriteSize,
            &ColliderExtents,
        ),
        Without<Platform>,
    >,
    plat_q: Query<
        (&Transform, &SpriteSize, &ColliderExtents, &Velocity, &PlatformKind),
        (With<Platform>, Without<Player>, Without<Ground>, Without<Goal>),
    >,
    ground_q: Query<
        (&Transform, &SpriteSize, &ColliderExtents),
        (With<Ground>, Without<Player>),
    >,
    mut goal_q: Query<
        (&mut Transform, &SpriteSize, &ColliderExtents),
        (With<Goal>, Without<Player>, Without<Ground>, Without<Platform>),
    >,
    mut link: ResMut<PlayerPlatformLink>,
) {
    for (mut p_tf, mut p_vel, mut player, p_size, p_ext) in &mut player_q {
        // Reset every frame
        player.is_grounded = false;
        player.is_on_platform = false;
        link.platform_velocity = Vec2::ZERO;

        let p_half = half_extents(p_size.0, p_ext.0);

        // ---- Platform collisions ----
        for (pl_tf, pl_size, pl_ext, pl_vel, _kind) in &plat_q {
            let p_pos = p_tf.translation.truncate();
            let pl_pos = pl_tf.translation.truncate();
            let pl_half = half_extents(pl_size.0, pl_ext.0);

            let player_bottom = p_pos.y - p_size.0.y / 2.0;
            // Platform top (offset by 10px like the original for static tall
            // platforms that use the -10 check)
            let platform_top = pl_pos.y + pl_size.0.y / 2.0 - 10.0;

            if let Some((axis, pen)) = aabb_overlap(p_pos, p_half, pl_pos, pl_half) {
                // Only land on top, not from sides/below
                if player_bottom >= platform_top {
                    p_tf.translation.x += axis.x * pen;
                    p_tf.translation.y += axis.y * pen;
                    // Keep 0.5px overlap so grounded persists next frame
                    if axis.y > 0.0 {
                        p_tf.translation.y -= 0.5;
                    }
                    p_vel.0.y = 0.0;

                    player.is_grounded = true;
                    player.is_on_platform = true;
                    player.gravity = 0.0;
                    link.platform_velocity = pl_vel.0;
                    break;
                } else {
                    // Side / bottom collision: push out
                    p_tf.translation.x += axis.x * pen;
                    p_tf.translation.y += axis.y * pen;
                    if axis.y < 0.0 {
                        p_vel.0.y = 0.0;
                    }
                }
            }
        }

        // ---- Ground collision ----
        for (g_tf, g_size, g_ext) in &ground_q {
            let g_pos = g_tf.translation.truncate();
            let g_half = half_extents(g_size.0, g_ext.0);
            let p_pos = p_tf.translation.truncate();

            if let Some((axis, pen)) = aabb_overlap(p_pos, p_half, g_pos, g_half) {
                p_tf.translation.x += axis.x * pen;
                p_tf.translation.y += axis.y * pen;
                // Keep 0.5px overlap so grounded persists next frame
                if axis.y > 0.0 {
                    p_tf.translation.y -= 0.5;
                }
                // Only zero velocity along collision axis (NOT horizontal!)
                p_vel.0.y = 0.0;
                player.gravity = 0.0;
                player.is_grounded = true;
                player.is_on_platform = false;
            }
        }

        // ---- Goal collision ----
        for (mut g_tf, g_size, g_ext) in &mut goal_q {
            let g_pos = g_tf.translation.truncate();
            let g_half = half_extents(g_size.0, g_ext.0);
            let p_pos = p_tf.translation.truncate();

            if let Some(_) = aabb_overlap(p_pos, p_half, g_pos, g_half) {
                g_tf.translation.x = 890.0; // move off screen
                player.end_game = true;
            }
        }

        // If in the air, re-enable gravity
        if !player.is_grounded {
            player.gravity = GRAVITY;
        }
    }
}

// ---------------------------------------------------------------------------
// Animation System
// ---------------------------------------------------------------------------

fn animation_system(
    time: Res<Time>,
    keys: Res<ButtonInput<KeyCode>>,
    mut query: Query<(&Player, &mut AnimState, &mut Sprite)>,
) {
    let dt = time.delta_secs();

    for (player, mut anim, mut sprite) in &mut query {
        let grounded = player.is_grounded;

        // Walking right
        if keys.pressed(KeyCode::KeyD) && grounded {
            if anim.min != 0 || anim.max != 3 {
                anim.min = 0;
                anim.max = 3;
                anim.start_timer = 0.1;
                anim.timer = 0.1;
            }
            anim.facing_right = true;
            update_anim_timer(&mut anim, dt);
        }
        // Walking left
        else if keys.pressed(KeyCode::KeyA) && grounded {
            if anim.min != 21 || anim.max != 24 {
                anim.min = 21;
                anim.max = 24;
                anim.start_timer = 0.1;
                anim.timer = 0.1;
                anim.idx = 21;
            }
            anim.facing_right = false;
            update_anim_timer(&mut anim, dt);
        }
        // Jumping / in-air
        else if !grounded {
            if anim.facing_right {
                anim.min = 4;
                anim.max = 4;
                anim.idx = 4;
            } else {
                anim.min = 25;
                anim.max = 25;
                anim.idx = 25;
            }
            // Allow mid-air direction change
            if keys.pressed(KeyCode::KeyD) {
                anim.facing_right = true;
                anim.idx = 4;
            } else if keys.pressed(KeyCode::KeyA) {
                anim.facing_right = false;
                anim.idx = 25;
            }
        }

        // Idle
        if !keys.pressed(KeyCode::KeyA) && !keys.pressed(KeyCode::KeyD) && grounded {
            if anim.facing_right {
                anim.min = 0;
                anim.max = 0;
                anim.idx = 0;
            } else {
                anim.min = 21;
                anim.max = 21;
                anim.idx = 21;
            }
        }

        // Apply frame to sprite atlas
        if let Some(ref mut atlas) = sprite.texture_atlas {
            atlas.index = anim.idx;
        }
    }
}

fn update_anim_timer(anim: &mut AnimState, dt: f32) {
    anim.timer -= dt;
    if anim.timer <= 0.0 {
        anim.idx += 1;
        anim.timer = anim.start_timer;
    }
    if anim.idx > anim.max {
        anim.idx = anim.min + 1;
    }
}

// ---------------------------------------------------------------------------
// End-Game / Fireworks System
// ---------------------------------------------------------------------------

/// When the player reaches the goal:
/// - Levels 1–19 → despawn level entities, reset player, spawn next level
/// - Level 20     → show fireworks & "You Win!"
fn endgame_system(
    mut commands: Commands,
    asset_server: Res<AssetServer>,
    mut level: ResMut<CurrentLevel>,
    mut player_q: Query<(
        &mut Player,
        &mut Transform,
        &mut Velocity,
        &mut Impulse,
        &mut Force,
        &mut AnimState,
        &mut Sprite,
    )>,
    level_entities: Query<Entity, With<LevelEntity>>,
    mut firework_q: Query<(&mut Transform, &mut Visibility), (With<Firework>, Without<WinText>, Without<Player>)>,
    mut text_q: Query<(&mut Visibility, &mut Text2d), (With<WinText>, Without<Firework>, Without<Player>)>,
) {
    let Ok((mut player, mut p_tf, mut vel, mut imp, mut force, mut anim, mut sprite)) =
        player_q.get_single_mut()
    else {
        return;
    };

    if !player.end_game {
        return;
    }

    if level.0 < 20 {
        // ---- Transition to next level ----

        // Despawn all level-specific entities (platforms, goal)
        for entity in &level_entities {
            commands.entity(entity).despawn();
        }

        // Reset the player
        player.end_game = false;
        player.is_grounded = false;
        player.is_on_platform = false;
        player.gravity = GRAVITY;
        p_tf.translation.x = 10.0;
        p_tf.translation.y = 21.0;
        vel.0 = Vec2::ZERO;
        imp.0 = Vec2::ZERO;
        force.0 = Vec2::ZERO;
        anim.idx = 0;
        anim.min = 0;
        anim.max = 0;
        anim.facing_right = true;
        if let Some(ref mut atlas) = sprite.texture_atlas {
            atlas.index = 0;
        }

        // Advance to next level
        level.0 += 1;
        spawn_level(&mut commands, &*asset_server, level.0);

        // Show level banner
        commands.spawn((
            Text2d::new(format!("Level {}", level.0)),
            TextFont {
                font_size: 72.0,
                ..default()
            },
            TextColor(Color::WHITE),
            Transform::from_xyz(400.0, 350.0, 10.0),
            LevelBanner(2.0),
        ));
    } else {
        // ---- All 20 levels complete – show fireworks ----
        let mut rng = rand::thread_rng();

        for (mut tf, mut vis) in &mut firework_q {
            *vis = Visibility::Visible;
            tf.translation.x = rng.gen_range(50.0..750.0);
            tf.translation.y = rng.gen_range(50.0..550.0);
        }

        for (mut vis, mut text) in &mut text_q {
            *vis = Visibility::Visible;
            *text = Text2d::new("You Win!");
        }
    }
}

/// Fade out the level banner after its timer expires.
fn level_banner_system(
    mut commands: Commands,
    time: Res<Time>,
    mut query: Query<(Entity, &mut LevelBanner, &mut TextColor)>,
) {
    let dt = time.delta_secs();
    for (entity, mut banner, mut color) in &mut query {
        banner.0 -= dt;
        if banner.0 <= 0.0 {
            commands.entity(entity).despawn();
        } else {
            // Fade out during the last second
            let alpha = banner.0.min(1.0);
            *color = TextColor(Color::srgba(1.0, 1.0, 1.0, alpha));
        }
    }
}

// ---------------------------------------------------------------------------
// Main
// ---------------------------------------------------------------------------

fn main() {
    App::new()
        .add_plugins(DefaultPlugins.set(WindowPlugin {
            primary_window: Some(Window {
                title: "Mario Platformer – Bevy".to_string(),
                resolution: (WINDOW_WIDTH, WINDOW_HEIGHT).into(),
                resizable: false,
                ..default()
            }),
            ..default()
        }))
        .add_systems(Startup, setup)
        .add_systems(
            Update,
            (
                player_input,
                platform_movement,
                physics_integration,
                collision_system,
                animation_system,
                endgame_system,
                level_banner_system,
            )
                .chain(),
        )
        .run();
}
