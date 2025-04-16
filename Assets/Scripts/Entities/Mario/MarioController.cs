using ObjectBasedStateMachine.UnLayered;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// The Input Reader/Controller object for the system. Placed on the Root of Mario's entity because the game is simple enough that it shouldn't need to keep track of Input with any kind of cross-scene stuff. <br />
/// This asset contains references to all the input actions. While this is overengineered for this particular project, the "easy" "Generate C# class" option is deceptively rigid in just the wrong way to make stuff like rebinding impossible without really obtuse and unreliable fixes. <br />
/// So, I've worked to canibalize the generated C# class to make something that, while having to be manually upkept with every change to the Input Asset, at least is properly connected with it and doesn't deliberately make itself a pain in the ass. <br />
/// Future versions of this solution will likely be based on ScriptableObject Singletons.
/// </summary>
[DefaultExecutionOrder(-10)]
public class MarioController : MonoBehaviour, ISingleton<MarioController>
{
    #region Singleton

    protected static MarioController I;
    protected ISingleton<MarioController> S => this;
    public static MarioController Get() => ISingleton<MarioController>.Get(ref I);
    public static bool TryGet(out MarioController result) => ISingleton<MarioController>.TryGet(Get, out result);

    #endregion

    #region InputActions

    public InputActionAsset asset;

    public Gameplay GameplayGroup;
    [Serializable]
    public struct Gameplay
    {
        public InputActionReference r_XMovement;
        public InputActionReference r_YMovement;
        public InputActionReference r_Jump;

        public static float XMovement => Get().GameplayGroup.r_XMovement.action.ReadValue<float>();
        public static float YMovement => Get().GameplayGroup.r_YMovement.action.ReadValue<float>();
        public static InputAction Jump => Get().GameplayGroup.r_Jump.action;
    }

    public UI UIGroup;
    [Serializable]
    public struct UI
    {
        public InputActionReference r_Pause;
        public static InputAction Pause => Get().UIGroup.r_Pause.action;
    }


    public void Enable() => asset.Enable();
    public void Disable() => asset.Disable();

    #endregion

    #region References
    public GameObject pauseIcon;
    StateMachine marioStateMachine;
    MarioPhysicsBody marioBody;
    #endregion

    public void Awake()
    {
        S.Initialize(ref I);
        asset.Enable();
        TryGetComponent(out marioStateMachine);
        TryGetComponent(out marioBody);
        Gameplay.Jump.performed += JumpAction;
        GameplayGroup.r_YMovement.action.performed += ClimbAction;
        UI.Pause.performed += TogglePause;
    }

    private void OnDestroy()
    {
        Gameplay.Jump.performed -= JumpAction;
        GameplayGroup.r_YMovement.action.performed -= ClimbAction;
        UI.Pause.performed -= TogglePause;
        S.DeInitialize(ref I);
    }

    private void JumpAction(InputAction.CallbackContext context) => marioStateMachine.SendSignal("Jump");

    private void ClimbAction(InputAction.CallbackContext context) => marioBody.ClimbAction(context.ReadValue<float>() < 0);

    public void Death()
    {
        CoroutinePlus C = new(Enum(), this);
        IEnumerator Enum()
        {
            Time.timeScale = 0;
            marioStateMachine.enabled = false;

            GetComponent<MarioSpriteManager>().GoDied();
            yield return WaitFor.SecondsRealtime(3);
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    public void Win()
    {
        CoroutinePlus C = new(Enum(), this);
        IEnumerator Enum()
        {
            Time.timeScale = 0;
            marioStateMachine.enabled = false;

            yield return WaitFor.SecondsRealtime(5);
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    bool paused;
    public void TogglePause(InputAction.CallbackContext context)
    {
        paused = !paused;
        Time.timeScale = paused ? 0 : 1;
        //marioStateMachine.enabled = paused ? false : true;
        //pauseIcon.SetActive(paused ? true : false);
        //Simplified the code for pausing to not involve conditional operator.
        marioStateMachine.enabled = !paused;
        pauseIcon.SetActive(paused);
    }
}
