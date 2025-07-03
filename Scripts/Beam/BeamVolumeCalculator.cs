using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamVolumeCalculator : MonoBehaviour
{
    [Header("Volume Calculation Settings")]
    public int samplePoints = 1000; // 부피 계산을 위한 샘플 포인트 수
    public float tolerance = 0.01f; // 겹침 판정을 위한 허용 오차
    
    public static float CalculateBeamOverlapVolume(Beam beam1, Beam beam2)
    {
        // 두 Beam의 위치와 방향 정보
        Vector3 pos1 = beam1.transform.position;
        Vector3 pos2 = beam2.transform.position;
        
        // Beam의 실제 방향 (localScale의 z 방향)
        Vector3 dir1 = beam1.transform.forward;
        Vector3 dir2 = beam2.transform.forward;
        
        float r1 = beam1.radius;
        float r2 = beam2.radius;
        float l1 = beam1.length;
        float l2 = beam2.length;
        
        // 두 Beam의 끝점 계산
        Vector3 end1 = pos1 + dir1 * l1;
        Vector3 end2 = pos2 + dir2 * l2;
        
        // 간단한 거리 기반 계산으로 변경
        return CalculateSimpleOverlap(pos1, end1, r1, pos2, end2, r2);
    }
    
    // 간단하고 정확한 겹침 부피 계산
    private static float CalculateSimpleOverlap(Vector3 start1, Vector3 end1, float r1,
                                               Vector3 start2, Vector3 end2, float r2)
    {
        // 두 Beam의 중심점 사이 거리
        float centerDistance = Vector3.Distance(start1, start2);
        
        // 두 반지름의 합
        float radiusSum = r1 + r2;
        
        // 겹치지 않는 경우
        if (centerDistance > radiusSum)
        {
            return 0f;
        }
        
        // 겹치는 경우 - 간단한 근사치 계산
        float overlapRadius = Mathf.Min(r1, r2);
        float overlapLength = Mathf.Min(Vector3.Distance(start1, end1), Vector3.Distance(start2, end2));
        
        // 겹치는 부피 = π * r² * h * 겹침 계수
        float overlapFactor = 1f - (centerDistance / radiusSum); // 거리에 따른 겹침 계수
        float overlapVolume = Mathf.PI * overlapRadius * overlapRadius * overlapLength * overlapFactor;
        
        return overlapVolume;
    }
    
    private static float CalculateMonteCarloOverlap(Vector3 start1, Vector3 end1, float r1, 
                                                   Vector3 start2, Vector3 end2, float r2)
    {
        int insideCount = 0;
        int totalSamples = 1000;
        
        // 두 Beam을 포함하는 바운딩 박스 계산
        Bounds bounds = new Bounds();
        bounds.Encapsulate(start1);
        bounds.Encapsulate(end1);
        bounds.Encapsulate(start2);
        bounds.Encapsulate(end2);
        
        // 반지름을 고려한 바운딩 박스 확장
        float maxRadius = Mathf.Max(r1, r2);
        bounds.Expand(maxRadius * 2);
        
        for (int i = 0; i < totalSamples; i++)
        {
            // 랜덤 포인트 생성
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );
            
            // 두 Beam 모두에 포함되는지 확인
            bool inBeam1 = IsPointInCylinder(randomPoint, start1, end1, r1);
            bool inBeam2 = IsPointInCylinder(randomPoint, start2, end2, r2);
            
            if (inBeam1 && inBeam2)
            {
                insideCount++;
            }
        }
        
        // 겹치는 부피 = (포함된 포인트 수 / 전체 포인트 수) * 전체 바운딩 박스 부피
        float overlapRatio = (float)insideCount / totalSamples;
        float totalVolume = bounds.size.x * bounds.size.y * bounds.size.z;
        
        return overlapRatio * totalVolume;
    }
    
    private static bool IsPointInCylinder(Vector3 point, Vector3 start, Vector3 end, float radius)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 toPoint = point - start;
        
        // 점에서 Beam 축까지의 거리 계산
        float projection = Vector3.Dot(toPoint, direction);
        
        // Beam 길이 범위 내에 있는지 확인
        if (projection < 0 || projection > Vector3.Distance(start, end))
        {
            return false;
        }
        
        // Beam 축에서의 거리 계산
        Vector3 closestPoint = start + direction * projection;
        float distance = Vector3.Distance(point, closestPoint);
        
        return distance <= radius;
    }
    
    // 두 실린더의 겹치는 부피를 더 정확하게 계산하는 방법
    public static float CalculateCylinderIntersectionVolume(Beam beam1, Beam beam2)
    {
        Vector3 pos1 = beam1.transform.position;
        Vector3 pos2 = beam2.transform.position;
        Vector3 dir1 = beam1.transform.forward;
        Vector3 dir2 = beam2.transform.forward;
        
        float r1 = beam1.radius;
        float r2 = beam2.radius;
        float l1 = beam1.length;
        float l2 = beam2.length;
        
        // 두 Beam의 끝점
        Vector3 end1 = pos1 + dir1 * l1;
        Vector3 end2 = pos2 + dir2 * l2;
        
        // 두 Beam이 평행한 경우
        if (Vector3.Dot(dir1, dir2) > 0.99f || Vector3.Dot(dir1, dir2) < -0.99f)
        {
            return CalculateParallelCylinderOverlap(pos1, end1, r1, pos2, end2, r2);
        }
        
        // 교차하는 경우 (더 복잡한 계산)
        return CalculateIntersectingCylinderOverlap(pos1, end1, r1, pos2, end2, r2);
    }
    
    private static float CalculateParallelCylinderOverlap(Vector3 start1, Vector3 end1, float r1,
                                                         Vector3 start2, Vector3 end2, float r2)
    {
        // 두 평행한 실린더 사이의 거리
        Vector3 direction = (end1 - start1).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
        
        float distance = Vector3.Distance(start1, start2);
        
        // 겹치는 길이 계산
        float overlapLength = Mathf.Min(Vector3.Distance(start1, end1), Vector3.Distance(start2, end2));
        
        // 겹치는 부피 (간단한 근사치)
        if (distance <= r1 + r2)
        {
            float overlapRadius = Mathf.Min(r1, r2);
            return Mathf.PI * overlapRadius * overlapRadius * overlapLength;
        }
        
        return 0f;
    }
    
    private static float CalculateIntersectingCylinderOverlap(Vector3 start1, Vector3 end1, float r1,
                                                             Vector3 start2, Vector3 end2, float r2)
    {
        // 교차하는 실린더의 겹치는 부피는 복잡하므로 Monte Carlo 방법 사용
        return CalculateMonteCarloOverlap(start1, end1, r1, start2, end2, r2);
    }
} 