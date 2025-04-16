using ObjectBasedStateMachine.UnLayered;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MarioMovementJumping : StateBehaviour
{
    #region Config
    public float horizontalSpeed;
    public float hAcceleration;
    public float hDecceleration;

    public float jumpPower;
    public float jumpMinHeight;
    public float jumpMaxHeight;
    public float gravity;
    public float terminalVelocity;
    public float fallDeathHeight;

    public bool allowTurnWhileJumping;
    public bool allowJumpCancel;
    #endregion
    #region References
    public InputActionReference r_inputHorizontal;
    public float inputHorizontal => r_inputHorizontal.action.ReadValue<float>();
    MarioPhysicsBody body;
    #endregion
    #region Data
    public enum JumpPhase
    {
        Inactive = -1,
        PreMinHeight,
        PreMaxHeight,
        SlowingDown,
        Falling
    }
    private JumpPhase jumpPhase = JumpPhase.Inactive;
    private float startHeight;
    private float targetMinHeight;
    private float targetSlowHeight;
    private float targetFallDeathHeight;
    #endregion

    public override void OnAwake()
    {
        Machine.TryGetComponent(out body);
        state.signals["Land"] += Land;
    }

    public override void OnFixedUpdate()
    {
        if (allowTurnWhileJumping)
        {
            float input = r_inputHorizontal.action.ReadValue<float>();

            if (input == 0f && body.velocity.x != 0)
                body.velocity.x = Mathf.MoveTowards(body.velocity.x, 0, hDecceleration);
            /*else if (input > 0)//right
                body.velocity.x = Mathf.MoveTowards(body.velocity.x, horizontalSpeed, hAcceleration);
            else if (input < 0)//left
                body.velocity.x = Mathf.MoveTowards(body.velocity.x, -horizontalSpeed, hAcceleration);*/
            //Removes one if-else statement, usable for either direction.
            else if (input != 0f)
                body.velocity.x = Mathf.MoveTowards(body.velocity.x, Mathf.Sign(input) * horizontalSpeed, hAcceleration);
        }

        if (jumpPhase == JumpPhase.PreMinHeight && body.position.y >= targetMinHeight) jumpPhase = JumpPhase.PreMaxHeight;
        if (jumpPhase == JumpPhase.PreMaxHeight && body.position.y >= targetSlowHeight) jumpPhase = JumpPhase.SlowingDown;

        if ((body.velocity.y <= 0 && jumpPhase is not JumpPhase.Falling) ||
            (jumpPhase is > JumpPhase.PreMinHeight and not JumpPhase.Falling && allowJumpCancel && !MarioController.Gameplay.Jump.IsPressed()))
        {
            jumpPhase = JumpPhase.Falling;
            body.velocity.y = body.velocity.y.Max(0);
            targetFallDeathHeight = body.position.y - fallDeathHeight;
        }

        if (jumpPhase < JumpPhase.SlowingDown) body.velocity.y = jumpPower;
        else body.velocity.y = (body.velocity.y - Time.fixedDeltaTime * gravity).Min(-terminalVelocity); 
    }

    /// <summary>
    /// Begins this Jumping State. Called by Signals.
    /// </summary>
    /// <param name="targetState">Set to Falling if walking off a platform</param>
    public void BeginJump(JumpPhase targetState = JumpPhase.PreMinHeight)
    {
        body.Grounded = false;
        Enter();
        switch (targetState)
        {             
            case JumpPhase.PreMinHeight:

                body.velocity.y = jumpPower;
                startHeight = body.position.y;
                targetMinHeight = startHeight + jumpMinHeight;
                targetSlowHeight = (startHeight + jumpMaxHeight) - (jumpPower.P() / (2 * gravity));

                if (targetSlowHeight <= body.position.y)
                {
                    body.velocity.y = Mathf.Sqrt(2 * gravity * targetSlowHeight);
                    targetMinHeight = transform.position.y;
                }
                break;

            case JumpPhase.Falling:
                targetFallDeathHeight = body.position.y - fallDeathHeight; 
                break;
            default: return;
        }
        jumpPhase = targetState;
    }

    private void Land()
    {
        if (body.position.y <= targetFallDeathHeight)
            Machine.SendSignal("Death", overrideReady: true);
    }

}
