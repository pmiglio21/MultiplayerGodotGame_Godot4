
namespace Enums
{
    public enum InputType
    {
        //Left joystick controls
        MoveEast,
        MoveNorth,
        MoveWest,
        MoveSouth,

        //UI actions
        UiActionConfirm, //South button, Space bar
        UiActionCancel, //East button, Tab button

        //In-Game actions
        GameActionPause, //Start button, Enter key
        GameActionAbilityActivate, //East button, Q key
        GameActionInteract, //South button, Space bar
        GameActionAttack1, //West button, M1
        GameActionAttack2, //North button, M2

        //Middle buttons
        SelectButton,

        //Item Loop buttons
        ItemLoopLeft, //Left shoulder, mouse scroll up
        ItemLoopRight, //Right shoulder, mouse scroll down

        //Shoulder buttons
        LeftShoulder,
        LeftTrigger,
        RightShoulder,
        RightTrigger,

        //D-Pad buttons
        DPadEast, 
        DPadNorth,
        DPadWest,
        DPadSouth
    }
}
