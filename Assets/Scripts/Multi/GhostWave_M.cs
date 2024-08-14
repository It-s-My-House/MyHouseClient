using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Photon.Pun;
using Photon.Realtime;

public class GhostWave_M : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject ghostPrefab;

    private IObjectPool<Monster> _pool;
    private Coroutine spawnCoroutine;

    public Transform ghostWavePosition;
    float spawnGhostInterval = 0;  // ���� ���� ���� / ó���� �ٷ� ����. �� ���Ŀ� 60�ʾ� �� �ִٰ� ����.
    int additionalSpawnGhostCount = 0;  // �߰� ������ ���� ��.

    int playerCountInGame; // �ΰ��ӿ� �ִ� �÷��̾� ��.

    private void Awake()
    {
        if (_pool == null)
        {
            _pool = new ObjectPool<Monster>(CreateMonster, OnGetMonster, OnReleaseMonster, OnDestroyMonster, maxSize: 100);
        }
    }

    private void Start()
    {
        // �÷��̾ �� ������ �ƴ��� Ȯ���Ͽ� �ڷ�ƾ ���� ���� ����.
        CheckPlayerCountAndStartCoroutine();
    }

    void Update()
    {
        // �÷��̾� �� ������Ʈ.
        playerCountInGame = PhotonNetwork.CurrentRoom.PlayerCount;

        // �÷��̾� ���� ���� �ڷ�ƾ ����.
        if (playerCountInGame == 1 && spawnCoroutine == null)
        {
            Debug.Log("�ο� �� 1��!!!!");
            spawnCoroutine = StartCoroutine(SpawnGhostsOverTime());
        }
        else if (playerCountInGame > 1 && spawnCoroutine != null)
        {
            Debug.Log("���� �ο��� 1�� �̻��� " + playerCountInGame + "��!!! ��ž �ڷ�ƾ �� ���� ����Ʈ ����");
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
            AllGhostDestroy();
        }
    }

    private IEnumerator SpawnGhostsOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnGhostInterval);

            additionalSpawnGhostCount++;
            spawnGhostInterval += 60f;

            for (int i = 0; i < additionalSpawnGhostCount; i++)
            {
                CreateMonster();
            }
        }
    }

    private Monster CreateMonster()
    {
        Vector3 randomPosition = ghostWavePosition.position + Random.insideUnitSphere * 7f;
        randomPosition.y = 0; // Ghost ���� �� position.y ���� 0�̵��� ����.

        GameObject ghost = Instantiate(ghostPrefab, randomPosition, Quaternion.identity);
        ghost.transform.SetParent(transform, false); // ���� ��ġ�� ��ȯ
        Monster monster = ghost.GetComponent<Monster>();
        if (monster != null)
        {
            monster.SetManagedPool(_pool);
        }
        else
        {
            Debug.LogError("ModifiedMonster_S component not found");
        }

        return monster;
    }

    private void OnGetMonster(Monster monster)
    {
        monster.gameObject.SetActive(true);
    }

    private void OnReleaseMonster(Monster monster)
    {
        monster.gameObject.SetActive(false);
    }

    private void OnDestroyMonster(Monster monster)
    {
        Destroy(monster.gameObject);
    }

    public void AllGhostDestroy()
    {
        // ��� ����Ʈ ������Ʈ�� ����.
        Monster[] allGhosts = FindObjectsOfType<Monster>();
        foreach (Monster ghost in allGhosts)
        {
            Destroy(ghost.gameObject);
        }
    }

    // �÷��̾ �� ���� �� ȣ��Ǵ� �ݹ�.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        // �÷��̾� �� ������ Ȯ���Ͽ� �ڷ�ƾ ����.
        CheckPlayerCountAndStartCoroutine();
    }

    private void CheckPlayerCountAndStartCoroutine()
    {
        playerCountInGame = PhotonNetwork.CurrentRoom.PlayerCount;

        if (playerCountInGame == 1 && spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnGhostsOverTime());
        }
        else if (playerCountInGame > 1 && spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
            AllGhostDestroy();
        }
    }
}