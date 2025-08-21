using SlimePirates.Model;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SlimePirates.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 3;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool isdoubljumponce = false;
        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        public PlatformerModel model = new PlatformerModel() { jumpDeceleration = 0f, jumpModifier = 0.9f };

        private CharacterController controller;

        private InputAction m_MoveAction;
        private InputAction m_JumpAction;
        private Vector2 moveInput;
        private float jumpInput;

        public Bounds Bounds => collider2d.bounds;

        public void Menu(InputAction.CallbackContext context)
        {
            //var MGC = GameObject.Find("GameController").GetComponent<MetaGameController>();
            //MGC.Menu(context);
            //Debug.LogError(context.ReadValue<float>());
            
        }

        public void Move(InputAction.CallbackContext context)
        {
            if (controlEnabled)
            {
                moveInput = context.ReadValue<Vector2>();
                move.x = moveInput.x;
                //Debug.LogError(move.x.ToString());
            } else
            {
                move.x = 0;
            }

        }
        public void Jump(InputAction.CallbackContext context)
        {

            if (controlEnabled)
            {
                jumpInput = context.ReadValue<float>();
                if (jumpState == JumpState.Grounded && jumpInput==1)
                    jumpState = JumpState.PrepareToJump;
                if (jumpState == JumpState.InFlight && jumpInput==1)
                {
                    IsDoubleJump = true;
                    animator.SetBool("IsDoubleJump", IsDoubleJump);
                }
                else if (jumpInput==1)
                {
                    stopJump = true;
                    //Schedule<PlayerStopJump>().player = this;
                }
            }
        }

        void Awake()
        {
            controller = GetComponent<CharacterController>();

            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            IsDoubleJump = false;
        }

        protected override void Update()
        {
            //foreach (var item in PlayerInput.all)
            //{
            //    if (item.name != "GameController") 
            //    {
            //        if (this.transform.position.x > item.transform.position.x)
            //            GameObject.Find("CM vcam1").GetComponent<CinemachineCamera>().Target.TrackingTarget = item.gameObject.transform;
            //    }

            //}

            UpdateJumpState();
            base.Update();

        }

        void UpdateJumpState()
        {
            //jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        //Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        //Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                Debug.LogError(velocity.y);
                jump = false;
                IsDoubleJump = false;
                animator.SetBool("isDoubleJump", IsDoubleJump);
                isdoubljumponce = false;
            }
            if (animator.GetBool("isDoubleJump"))
            {
                if (isdoubljumponce == false)
                {
                    velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                    stopJump = true;
                    IsDoubleJump = true;
                    isdoubljumponce = true;
                }
            }

            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}