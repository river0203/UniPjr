using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("캐릭터의 이동 속도 m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("캐릭터의 달리기 속도 m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("얼마나 빨리 캐릭터가 회전하는지")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("가속 및 감속")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("플레이어가 점프할 수 있는 높이")]
        public float JumpHeight = 1.2f;

        [Tooltip("캐릭터의 자체 중력값. 엔진 기본값은 -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("다시 점프할 수 있기 전에 필요한 시간. 즉시 다시 점프하려면 0f로 설정")]
        public float JumpTimeout = 0.50f;

        [Tooltip("낙하 상태에 들어가기 전에 필요한 시간. 계단을 내려갈 때 유용")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("캐릭터가 땅에 있는지 아닌지를 확인. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("거친 땅에 유용")]
        public float GroundedOffset = -0.14f;

        [Tooltip("땅에 닿았는지 체크할 범위. CharacterController의 radius와 값을 맞춰야한다.")]
        public float GroundedRadius = 0.28f;

        [Tooltip("어떤 레이어를 캐릭터가 Ground로 사용할 것인가")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("Cinemachine Virtual Camera가 따라다닐 카메라 타겟")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("얼마나 카메라를 올릴 수 있는지")]
        public float TopClamp = 70.0f;

        [Tooltip("얼마나 카메라를 내릴 수 있는지")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("모든 축에서 카메라 위치 고정")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
            return false;
#endif
            }
        }


        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            // CinemachineCamera가 따라다니는 타켓의 y축 회전각도 저장
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            // TryGetComponent는 Animator가 존재할 경우 1반환, out은 Animator가 존재할 경우 _animator에 Animator반환
            _hasAnimator = TryGetComponent(out _animator);

            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            _playerInput = GetComponent<PlayerInput>();
#else
         Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        // 애니메이션의 해시코드를 할당(해시코드 사용시 
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // 캐릭터가 땅에 닿았는지 확인하기 위해 캐릭터의 발 아래에 Vector 생성
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);

            /* CheckSphere(충돌체의 위치, 충돌체의 지름, 충돌 검출에 사용될 레이어 마이크, 트리거 충돌 처리 결정)
             * 충돌이 있을 경우 true값 반환 */
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore); // .Ignore: 트리거 충돌을 무시하고 물리적인 충돌만 처리

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                // 현재 디바이스가 마우스라면 1.0반환, 아니면 Time.deltaTime반환
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                //카메라의 수평, 수직 회전각도 변경
                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // 카메라 앵글이 360도를 넘어가지 않도록 제한
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);

            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // 시네머신 카메라 각도 = x : 현재 카메라 위치 + 기본 카메라 앵글값 / y : 현재 y 각도 / z : 0.0f 
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // 달리기 상태에 따라 스피드를 변경
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // 입력이 없을 시, 스피드 = 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // 캐릭터의 수평 속도 벡터값 할당 
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // 목표 속도보다 현재 속도가 적거나 많을 경우 가속 혹은 감속
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // 선형 결과보다 더 유기적인 속도 변화를 주는 곡선 결과를 만든다 
                // Lerp 의 T는 고정되어있으므로, 속도를 고정할 필요가 없다.
                // currentHorizontalSpeed, targetSpeed * inputMagnitude간의 차이가 있을 경우 목표 속도까지 시간에 따라 자연스럽게 증가
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // _speed를 소수점 셋째 자리에서 반올림
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // 입력 방향 정규화(벡터값 1)
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // 참고: 벡터2의 != 연산자는 근사값을 사용하므로 부동소수점 오류가 발생하지 않는다, 그리고 더 빠르다 
            // 이동 입력이 있는 경우 플레이어가 움직일 때 플레이어 회전
            if (_input.move != Vector2.zero)
            {
                // x, y의 입력 방향을 받아 Rad2Deg를 곱해 도 단위로 변환 + 카메라 y값 더하기
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;

                // SmoothDampAngle(현재 회전값, 목표 회전값, 현재 회전 속도, 증감 시간)
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // 카메라 위치를 기준으로 입력 방향으로 회전
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            // targetDirection = 회전값만큼의 전방 방향 벡터
            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // CharacterController.Move = 지정된 벡터만큼 캐릭터 이동
            // 타겟의 x, z벡터값 + 
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // 낙하 상태에 들어가기 위한 시간 초기화
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // 바닥에 닿았을 때 속도가 무한히 떨어지는 것 방지
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // Sqrt(H * -2  * G) = 원하는 높이에 도달하는 속도
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // 점프 재사용 대기시간이 0보다 클경우 시간에 재사용 대기시간이 줄어듦
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // 점프 재사용 대기시간 초기화
                _jumpTimeoutDelta = JumpTimeout;

                // 낙하 상태에 들어가기 전 시간 계산
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // 땅에 닿지 않으면 점프할 수 없음
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        //카메라 앵글 최소, 최대값 제한
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}