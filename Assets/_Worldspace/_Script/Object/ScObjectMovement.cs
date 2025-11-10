using _Workspace._Scripts.Managers;
using UnityEngine;

namespace _Workspace._Scripts.Object
{
    public class ScObjectMovement : MonoBehaviour
    {
        [Header("Gravity & Spin")] 
        [SerializeField] private float gravityScale = 1f;
        [SerializeField] private float maxUpVelocity = 12f;
        [SerializeField] private float spinDegPerSec;

        [Header("Bounce Setting")] 
        [SerializeField] private float restitution = 0.6f;
        [SerializeField] private float tangentDamping = 0.2f;
        [SerializeField] private float extraUpImpulse = 1f;
        [SerializeField] private float minLeftSpeed = 1.5f;
        
        private float _coolDown;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.isKinematic = false;
            _rb.linearDamping = 0f;
            _rb.angularDamping = 0.05f;
            _rb.constraints = RigidbodyConstraints.FreezePositionZ
                             | RigidbodyConstraints.FreezeRotationX
                             | RigidbodyConstraints.FreezeRotationY;
        }

        private void FixedUpdate()
        {
            _rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
        }

        public void LaunchTo(Vector3 targetPos, float horizontalSpeed, float targetYJitter = 0.1f)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            Vector3 p0 = transform.position;
            float dx = p0.x - targetPos.x;
            dx = Mathf.Max(dx, 0.05f);

            float vx = Mathf.Abs(horizontalSpeed);
            float t = dx / vx;

            float g = Physics.gravity.y * Mathf.Max(0.0001f, gravityScale);
            float y0 = p0.y;
            float yT = targetPos.y + Random.Range(-targetYJitter, targetYJitter);

            float v0Y = (yT - y0 - 0.5f * g * t * t) / t;

            v0Y = Mathf.Clamp(v0Y, -maxUpVelocity, maxUpVelocity);

            Vector3 v0 = new Vector3(-vx, v0Y, 0f);

            _rb.AddForce(v0 * _rb.mass, ForceMode.Impulse);

            if (spinDegPerSec == 0f) return;
            float spinRad = spinDegPerSec * Mathf.Deg2Rad;
            _rb.AddTorque(Vector3.forward * (spinRad * _rb.mass * 0.25f), ForceMode.Impulse);
        }

        private void OnCollisionEnter(Collision col)
        {
            if (!col.collider.CompareTag("PlayerHead")) return;
            ScAudioManager.instance.PlaySfx("Hit");
            if (SCEventbus.Instance.IsDiving) return;

            Vector3 v = _rb.linearVelocity;
            if (v.sqrMagnitude < 0.0001f) return;
            
            var contact = col.GetContact(0);
            Vector3 n = contact.normal.normalized;

            float vNmag = Vector3.Dot(v, n);
            Vector3 vN = vNmag * n;
            Vector3 vT = v - vN;

            Vector3 vAfter = (-restitution * vN) + ((1f - tangentDamping) * vT);

            if (vAfter.x > -minLeftSpeed)
                vAfter.x = -minLeftSpeed;

            _rb.linearVelocity = vAfter;

            if (extraUpImpulse > 0f)
                _rb.AddForce(Vector3.up * extraUpImpulse * _rb.mass, ForceMode.Impulse);
        }
    }
}

