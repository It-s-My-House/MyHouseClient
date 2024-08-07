using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStatus_S : MonoBehaviour
{
    #region 상태 및 능력치
    //[field: SerializeField] public Define.Role Role = Define.Role.None; // 몬스터도 시체가 되니까 Role 필요하려나? -> Yes: 이 코드 활성화, No: 이 코드 지우기
    [field: SerializeField] public float Hp { get; set; } = 100;    // 체력
    [field: SerializeField] public float MaxHp { get; private set; } = 100; // 최대 체력
    // 방어력 없앴음
    #endregion

    #region 애니메이션 및 피해
    Animator _animator;
    List<Renderer> _renderers;
    #endregion

    #region 점수 표시용
    #endregion
    


    void Awake()
    {
        _animator = GetComponent<Animator>();

        // 렌더 가져오기
        _renderers = new List<Renderer>();
        Transform[] underTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < underTransforms.Length; i++)
        {
            Renderer renderer = underTransforms[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                _renderers.Add(renderer);
                // if (renderer.material.color == null) Debug.Log("왜 색이 널?");
            }
        }
    }

    void Update()
    {
        Dead();
    }

    /// <summary>
    /// 데미지 입기
    /// </summary>
    /// <param name="attack"> 가할 공격력 </param>
    public void TakedDamage(int attack)
    {
        // 피해가 음수라면 회복되는 현상이 일어나므로 피해의 값을 0이상으로 되게끔 설정
        float damage = Mathf.Max(0, attack);
        Hp -= damage;

        Debug.Log(gameObject.name + "(이)가 " + damage + " 만큼 피해를 입었음!");
        Debug.Log("남은 체력: " + Hp);
    }

    /// <summary>
    /// 최대 체력의 0.2만큼 회복
    /// </summary>
    public void Heal()
    {
        // 현재 체력이 최대 체력보다 작을 때만 회복 적용
        if (Hp < MaxHp)
        {
            // 회복량
            float healAmount = MaxHp * 0.2f;

            // 회복량과 현재 체력과의 합이 최대 체력을 넘지 않도록 조절
            float healedAmount = Mathf.Clamp(Hp + healAmount, 0, MaxHp) - Hp;

            Debug.Log("이전 체력" + Hp);
            // 체력 회복
            Hp += healedAmount;
            Debug.Log("체력을 " + healedAmount + "만큼 회복!");
            Debug.Log("현재 체력: " + Hp);
        }
        else
        {
            Debug.Log("최대 체력. 회복할 필요 없음.");
        }
    }

    /// <summary>
    /// 사망
    /// </summary>
    public void Dead()
    {
        if (Hp <= 0)
        {
            _animator.SetTrigger("setDie");
            //Role = Define.Role.None; // 몬스터도 시체가 되니까 Role 필요하려나? -> Yes: 이 코드 활성화, No: 이 코드 지우기
            StartCoroutine(DeadSinkCoroutine());
        }
    }

    /// <summary>
    /// 시체 바닥으로 가라앉기
    /// </summary>
    /// <returns></returns>
    IEnumerator DeadSinkCoroutine()
    {
        yield return new WaitForSeconds(3f);
        while (transform.position.y > -1.5f)
        {
            transform.Translate(Vector3.down * 0.1f * Time.deltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// 피해 받으면 Material 붉게 변화
    /// </summary>
    public void HitChangeMaterials()
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.color = Color.red;
            Debug.Log("색 변한다.");
            //Debug.Log(_renderers[i].material.name);
        }

        StartCoroutine(ResetMaterialAfterDelay(1.7f));
        Debug.Log("공격받은 측의 체력:" + Hp);
    }

    /// <summary>
    /// 피해 받고 Material 원래대로 복구
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator ResetMaterialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < _renderers.Count; i++)
            _renderers[i].material.color = Color.white;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee" || other.tag == "Gun") // Melee나 Gun에 닿이면 색깔 바뀌도록 HitChangeMaterials() 호출
            HitChangeMaterials();
    }

    public void SetRoleAnimator(RuntimeAnimatorController animController, Avatar avatar)
    {
        _animator.runtimeAnimatorController = animController;
        _animator.avatar = avatar;

        // 애니메이터 속성 교체하고 껐다가 켜야 동작함
        _animator.enabled = false;
        _animator.enabled = true;
    }
}
