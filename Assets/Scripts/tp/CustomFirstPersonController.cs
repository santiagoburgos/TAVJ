using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

#pragma warning disable 618, 649
namespace UnityStandardAssets.Characters.FirstPerson
{

    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class CustomFirstPersonController : MonoBehaviour
    {
                // the sound played when character touches back on ground.

        private Camera m_Camera;
        
        private float m_YRotation;
        private Vector2 m_Input;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
 
        
        public bool m_Jump;
        public float horizontal = 0;
        public float vertical = 0;

        public bool walking = false;
        public bool shooting = false;
        public bool reloading = false;

        public float inputPlayerNumber = 0;
        
        
        
        //
        //custom
        private CharacterController controller;
        private Vector3 playerVelocity;
        private bool groundedPlayer;
        private float playerSpeed = 7.0f;
        private float jumpHeight = 3.0f;
        private float gravityValue = -9.81f;
        public int id;

        
        
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_Jumping = false;
         
            controller = gameObject.GetComponent<CharacterController>();
            
        }
        

        public void _FixedUpdate()
        {
            if (controller != null)
            {

                groundedPlayer = controller.isGrounded;


                if (groundedPlayer && playerVelocity.y < 0)
                {
                    playerVelocity.y = 0f;
                }

                Vector3 move = new Vector3(horizontal, 0, vertical);
                move = transform.TransformDirection(move);

                controller.Move(move * Time.deltaTime * playerSpeed);

                if (m_Jump && groundedPlayer)
                {
                    playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
                }

                playerVelocity.y += gravityValue * Time.deltaTime;
                controller.Move(playerVelocity * Time.deltaTime);

            }
        }
        


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
         
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
