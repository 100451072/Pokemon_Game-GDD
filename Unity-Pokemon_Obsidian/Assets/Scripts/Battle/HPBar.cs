using System.Collections;
using UnityEngine;

namespace Pokemon.Battle
{
    public class HPBar : MonoBehaviour
    {
        [SerializeField] private GameObject health;

        public void SetHp(float hpNormalized)
        {
            health.transform.localScale = new Vector3(hpNormalized, 1f);
        }

        public IEnumerator SetHpSmooth(float newHp)
        {
            var curHp = health.transform.localScale.x;
            var changeAmt = curHp - newHp;

            while (curHp - newHp > Mathf.Epsilon)
            {
                curHp -= changeAmt * Time.deltaTime;
                health.transform.localScale = new Vector3(curHp, 1f);
                yield return null;
            }
            health.transform.localScale = new Vector3(newHp, 1f);
        }
    }
}
