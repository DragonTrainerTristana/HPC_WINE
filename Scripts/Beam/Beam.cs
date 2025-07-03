using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour
{
    [Header("Beam Properties")]
    public float radius = 1f;
    public float length = 10f;
    
    [Header("Collision Info")]
    [SerializeField] private List<Beam> collidingBeams = new List<Beam>();
    [SerializeField] private float totalOverlapVolume = 0f;
    
    [Header("Public Info (Inspector에서 확인 가능)")]
    [SerializeField] public float overlapVolume = 0f;
    [SerializeField] public int collisionCount = 0;
    
    [Header("Debug")]
    public bool showCollisionInfo = false;
    
    private MeshRenderer meshRenderer;
    private Color originalColor;
    
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
        
        // Collider 추가
        if (GetComponent<Collider>() == null)
        {
            CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            capsuleCollider.radius = radius;
            capsuleCollider.height = length;
            capsuleCollider.isTrigger = true;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        Beam otherBeam = other.GetComponent<Beam>();
        if (otherBeam != null && otherBeam != this)
        {
            if (!collidingBeams.Contains(otherBeam))
            {
                collidingBeams.Add(otherBeam);
                CalculateOverlapVolume(otherBeam);
                UpdateVisualFeedback();
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        Beam otherBeam = other.GetComponent<Beam>();
        if (otherBeam != null)
        {
            collidingBeams.Remove(otherBeam);
            UpdateVisualFeedback();
        }
    }
    
    void CalculateOverlapVolume(Beam otherBeam)
    {
        // 두 Beam 사이의 겹치는 부피 계산 (더 정확한 방법 사용)
        float overlapVolume = BeamVolumeCalculator.CalculateBeamOverlapVolume(this, otherBeam);
        totalOverlapVolume += overlapVolume;
        
        // Inspector에서 확인할 수 있도록 public 변수로 설정
        if (showCollisionInfo)
        {
            Debug.Log($"Beam {name}과 {otherBeam.name} 사이의 겹치는 부피: {overlapVolume:F2} cubic units");
            Debug.Log($"  - Beam1 위치: {transform.position}, 반지름: {radius}, 길이: {length}");
            Debug.Log($"  - Beam2 위치: {otherBeam.transform.position}, 반지름: {otherBeam.radius}, 길이: {otherBeam.length}");
        }
    }
    
    void UpdateVisualFeedback()
    {
        // Public 변수 업데이트
        overlapVolume = totalOverlapVolume;
        collisionCount = collidingBeams.Count;
        
        if (meshRenderer != null)
        {
            if (collidingBeams.Count > 0)
            {
                // Collision이 있을 때 파란색으로 변경
                meshRenderer.material.color = Color.blue;
            }
            else
            {
                // Collision이 없을 때 원래 색상으로 복원
                meshRenderer.material.color = originalColor;
            }
        }
    }
    
    // Inspector에서 클릭할 때 호출되는 메서드
    [ContextMenu("Show Collision Info")]
    public void ShowCollisionInfo()
    {
        Debug.Log($"=== Beam {name} Collision Info ===");
        Debug.Log($"총 겹치는 부피: {totalOverlapVolume:F2} cubic units");
        Debug.Log($"Colliding Beams 수: {collidingBeams.Count}");
        
        for (int i = 0; i < collidingBeams.Count; i++)
        {
            if (collidingBeams[i] != null)
            {
                Debug.Log($"  - {collidingBeams[i].name}");
            }
        }
    }
    
    // Inspector에서 버튼으로 호출할 수 있는 메서드
    [Header("Inspector Buttons")]
    [SerializeField] private bool showInfo = false;
    
    void OnValidate()
    {
        if (showInfo)
        {
            ShowCollisionInfo();
            showInfo = false; // 버튼 클릭 후 자동으로 false로 설정
        }
    }
    
    // Public getter for Inspector
    public float GetTotalOverlapVolume()
    {
        return totalOverlapVolume;
    }
    
    public int GetCollidingBeamCount()
    {
        return collidingBeams.Count;
    }
    
    public List<Beam> GetCollidingBeams()
    {
        return collidingBeams;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == this.gameObject) return;
        if (other.CompareTag("Beam"))
        {
            Bounds a = GetComponent<Collider>().bounds;
            Bounds b = other.bounds;

            if (a.Intersects(b))
            {
                Bounds overlap = GetOverlapBounds(a, b);
                float thisOverlap = overlap.size.x * overlap.size.y * overlap.size.z;
                overlapVolume = thisOverlap;
                collisionCount = 1;
            }
            else
            {
                overlapVolume = 0f;
                collisionCount = 0;
            }
        }
    }

    Bounds GetOverlapBounds(Bounds a, Bounds b)
    {
        Vector3 min = Vector3.Max(a.min, b.min);
        Vector3 max = Vector3.Min(a.max, b.max);
        return new Bounds((min + max) / 2, max - min);
    }
} 