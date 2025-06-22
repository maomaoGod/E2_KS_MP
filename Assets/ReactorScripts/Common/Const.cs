
public class Consts
{
    // RPC IDs
    public class RPC
    {
        //--------------------Connection RPC Consts----------------------//
            public const uint NUM_ROOMS = 0;
            public const uint START_ROOM = 1;
            public const uint JOIN_ROOM = 2;
            public const uint GET_ROOM = 3;
            public const uint GET_PLAYER = 4;
            public const uint START_GAME = 5;
            public const uint STOP_ROOM = 6;
            public const uint ON_START_ROOM = 7;
            public const uint ON_GET_ROOM = 8;
            public const uint CLUSTER_STOP_ROOM = 9;

        //--------------------Chat System RPC Consts---------------------//
            public const uint ON_SEND_MESSAGE = 10;
            public const uint BROADCAST_MESSAGE = 11;

        //--------------------Avatar Sync RPC Consts---------------------//
            public const uint TRANSFORM = 12;
            public const uint ANIMATION_STATE = 13;
            public const uint ANIMATION_PARAM = 14;
            public const uint ANIMATION_TRIGGER = 15;
            public const uint TRANSFORM_RESPONSE = 16;

        //---------------------Building RPC Consts-----------------------//
            public const uint SPAWN_BUILDING_SERVER = 18;
            public const uint EDIT_BUILDING_SERVER = 19;
            public const uint REMOVE_BUILDING_SERVER = 20;

        //---------------------Friend System RPC Consts-------------------------//
            public const uint SEND_INVITE = 21;
            public const uint UPDATE_INVITE = 22;
            public const uint REGISTER_PLAYER_ID = 23;
            public const uint ACCEPT_INVITE = 24;
            public const uint DECLINE_INVITE = 25;
            public const uint UPDATE_FRIEND_LIST = 26;
            public const uint UPDATE_ALL_LISTS = 27;
            public const uint ON_UPDATE_ALL_LISTS = 28;
            public const uint ON_GET_UPDATE_ALL_LISTS = 29;
            public const uint REMOVE_FRIEND = 30;
            public const uint USER_NOT_EXIST = 31;

        //---------------------Avatar System RPC Consts-------------------//
            public const uint AVATAR_SPAWN_REQUEST = 47;
            public const uint SAVE_AVATAR_INFO = 48;

        //---------------------Car System RPC Consts----------------------//
            public const uint CAR_TRANSFORM = 32;
            public const uint CAR_FRONT_WHEEL_TRANSFORM = 33;
            public const uint CAR_BACK_WHEEL_TRANSFORM = 34;
            public const uint CAR_SPAWN_REQUEST = 35;
            public const uint REMOVE_ENTITY_REQUEST = 37;
            public const uint CAR_FRONT_WHEELY_TRANSFORM = 39;

        //---------------------Cydroid System RPC Consts------------------//
            public const uint CYDROID_TRANSFORM = 42;
            public const uint CYDROID_SPAWN_REQUEST = 43;
            public const uint CYDROID_RMOVE_REQUEST = 44;
            public const uint CYDROID_INDEX_SAVE_REQUEST = 45;
            public const uint CYDROID_INDEX_CHANGED = 46;

        //---------------------Mini Game System RPC Consts------------------//
            public const uint ETHER_SPAWN_REQUEST = 51;
            public const uint ETHER_REMOVE_REQUEST = 52;

            public const uint MONSTER_SPAWN_REQUEST = 55;
            public const uint MONSTER_HEALTH_CHANGE = 56;
            public const uint MONSTER_ALL_REMOVE = 57;
            public const uint MONSTER_RECEIVE_DAMAGE = 58;

            public const uint MONSTER_MINIGAME_START = 59;
            public const uint MONSTER_MINIGAME_END = 60;
            public const uint MONSTER_MINIGAME_WAVE = 61;

            public const uint MENTAR_RECEIVE_DAMAGE = 62;
            public const uint MENTAR_HEALTH_CHANGE = 63;
            public const uint MENTAR_SPAWN_REQUEST = 64;

            //----------------------Weapon RPC-------------------------------//
            public const uint SPAWN_WEAPON = 101;
            public const uint EQUIP_WEAPON = 102;
            public const uint UNEQUIP_WEAPON = 103;
            public const uint WEAPON_SOUND = 104;

            public const uint HEALTH_CHANGE = 111;

        //---------------------Magic attack RPC Consts------------------//
            public const uint SKY_FALL = 201;
            public const uint SKY_FALL_CHARGE = 202;
            public const uint SKY_FALL_CHARGE_END = 203;
            public const uint CHARGE_STAMINA = 204;
            public const uint CHARGE_STAMINA_END = 205;
            public const uint DASH_ATTACK = 206;
            public const uint DASH_ATTACK_END = 207;
            public const uint DASH_CHARGE = 208;
            public const uint DASH_CHARGE_END = 209;
            public const uint COMBO = 210;
            public const uint DASH_STRICK_HOLD = 211;
            public const uint BLIZZARD_START = 212;
            public const uint BLIZZARD_END = 213;
            public const uint PYROBLAST_CHARGE_START = 214;
            public const uint PYROBLAST_CHARGE_END = 215;
            public const uint LIGHTNING_START = 216;
            public const uint LIGHTNING_END = 217;

        //---------------------Sword Magic attack RPC Consts------------------//
            public const uint DASH_STRICK = 231;
            public const uint DASH_STRICK_END = 232;
            public const uint COMBO_STRICK = 233;
            public const uint COMBO_STRICK_END = 234;
            public const uint SKYFALL_STRICK = 235;
            public const uint SKYFALL_STRICK_END = 236;
            public const uint MAGIC_PROJECTILE_SPAWN = 237;
            public const uint MAGIC_PROJECTILE_FIRE = 238;

        //---------------------Attack Ragdoll RPC Consts------------------//
            public const uint MONSTER_RAGDOLL_START = 261;
            public const uint RAGDOLL_CHANGE = 262;
            
        //---------------------Monster VFX RPC Consts------------------//
            public const uint MONSTER_DEATH = 281;
            
            public const uint PlayerInput = 0;
    }

    public class Prop
    {
        public const uint NAME = 0;
        public const uint OWNER = 0;
        public const uint STATE = 1;
        public const uint STANCE = 2;
        public const uint MOVEMENT = 3;
        public const uint AVATAR_INDEX = 4;
        public const uint USERNAME = 5;
        public const uint POSITION = 6;
        public const uint ROTATION = 7;
        public const uint OWNER_NAME = 8;
        public const uint PLAYER_ID = 9;
        public const uint FRONT_WHEEL = 10;
        public const uint BACK_WHEEL = 11;
        public const uint AVATAR_CHANGE_SIGNAL = 12;
        public const uint FRONT_WHEELY = 13;
        public const uint CYDROID_INDEX = 14;
        public const uint AVATAR_ID = 15;
        public const uint Ether_ID = 16;
        public const uint MONSTER_HEALTH = 17;

        
        public const uint ENTITYTYPE = 18;
        public const uint BUDDYPLAYER = 19;
        public const uint BULLETTRACK = 20;
        public const uint MOVEDIRECTION = 21;
        public const uint HP = 22;
        
        public const uint HOLSTER_WEAPON1 = 100;
        public const uint HOLSTER_WEAPON2 = 101;
        public const uint HOLSTER_WEAPON3 = 102;
        public const uint HOLSTER_WEAPON4 = 103;
        public const uint EQUIP_WEAPON = 104;
        public const uint HEALTH = 110;

        public const uint RAGDOLL_STATE = 151;

        // 1000-1099 is reserved for animation states (one per layer)
        public const uint ANIMATION_STATES = 1000;
        // 1100+ is reserved for animation parameters
        public const uint ANIMATION_PARAMS = 1100;
        


        
    }

    
    
    public enum EntityType
    {
        E_Entity_None,
        E_Entity_Player,
        E_Entity_NPC,
        E_Entity_Bullet,
        E_Entity_FollowPlayer
    }

    public enum BulletTrack
    {
        E_Track_Linear,
        E_Track_Follow
    }
    
    public class Constant
    {
        public const string WORLD_NAME = "World";
        public const string DEFAULT_AVATAR_LINK = "https://api.readyplayer.me/v1/avatars/638df693d72bffc6fa17943c.glb";
    }

    public class LobbyInfo
    {
        public int lobbyPlayerCount = 0;
    }
}
