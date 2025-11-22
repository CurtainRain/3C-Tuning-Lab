using UnityEngine;

namespace GameRuntime.CustomUtils{
    public static class MathUtil{
        static readonly Collider[] _overlaps = new Collider[10];

        /// <summary>
        /// 获取包围点的所有 collider
        /// </summary>
        /// <param name="point">点</param>
        /// <param name="layerMask">LayerMask</param>
        /// <param name="overlaps">collider数组</param>
        /// <param name="radius">指定点检测的半径</param>
        /// <returns>collider数量</returns>
        public static int GetInsideColliders(Vector3 point, LayerMask layerMask, out Collider[] overlaps, float radius = 0.1f){
            int count = Physics.OverlapSphereNonAlloc(
                point,
                radius,
                _overlaps,
                layerMask,
                QueryTriggerInteraction.Ignore
            );
            overlaps = _overlaps;
            return count;
        }


        static readonly RaycastHit[] _rayHits = new RaycastHit[20];

        /// <summary>
        /// 尝试获取起点到距离终点最近的包围盒的进入点
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <param name="layerMask">LayerMask</param>
        /// <param name="entryPoint">碰撞点</param>
        /// <returns>是否成功</returns>
        public static bool TryGetEntryPoint(
            Vector3 startPoint,
            Vector3 endPoint,
            LayerMask layerMask,
            out Vector3 entryPoint,
            float endPointDetectDistance = 0.1f)
        {
            entryPoint = Vector3.zero;

            // 起点终点重合 直接返回
            Vector3 dir = endPoint - startPoint;
            float dist = dir.magnitude;
            if (dist <= Mathf.Epsilon) return false;


            // 获取终点所在的所有 collider
            int insideCount = GetInsideColliders(endPoint, layerMask, out var overlaps,endPointDetectDistance);
            if (insideCount == 0) return false;


            dir /= dist; // 归一化

            // 从起点到终点做 RaycastNonAlloc
            int hitCount = Physics.RaycastNonAlloc(
                startPoint,
                dir,
                _rayHits,
                dist,
                layerMask,
                QueryTriggerInteraction.Ignore
            );
            if (hitCount == 0) return false;


            // 在所有 hitPoint中，取离 startPoint 最近的那个 entryPoint
            bool found = false;
            float bestDist = float.PositiveInfinity;

            for (int i = 0; i < hitCount; i++)
            {
                var hit = _rayHits[i];

                // 检查这个 collider 是包围重点的colliders之一
                bool isInsideCol = false;
                for (int j = 0; j < insideCount; j++)
                {
                    if (hit.collider == overlaps[j])
                    {
                        isInsideCol = true;
                        break;
                    }
                }
                if (!isInsideCol) continue;

                if (hit.distance < bestDist)
                {
                    bestDist = hit.distance;
                    entryPoint = hit.point;
                    found = true;
                }
            }

            return found;
        }
    }
}
