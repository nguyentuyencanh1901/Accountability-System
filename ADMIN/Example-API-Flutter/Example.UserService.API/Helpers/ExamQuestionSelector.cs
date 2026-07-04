using Example.Common.Enums;
using Example.UserService.API.Entities;

namespace Example.UserService.API.Helpers
{
    /// <summary>Chọn ngẫu nhiên câu hỏi theo lĩnh vực, phân bổ độ khó và tổng điểm yêu cầu.</summary>
    public static class ExamQuestionSelector
    {
        public static List<Question>? TrySelectRandomQuestions(
            IReadOnlyList<Question> pool,
            int easyCount,
            int mediumCount,
            int hardCount,
            int requiredTotalPoints)
        {
            if (pool == null || pool.Count == 0 || requiredTotalPoints <= 0)
                return null;

            var totalNeeded = easyCount + mediumCount + hardCount;
            if (totalNeeded <= 0)
                return null;

            var easyPool = pool.Where(q => q.DifficultyLevel == (int)DifficultyLevelEnum.Easy).ToList();
            var mediumPool = pool.Where(q => q.DifficultyLevel == (int)DifficultyLevelEnum.Medium).ToList();
            var hardPool = pool.Where(q => q.DifficultyLevel == (int)DifficultyLevelEnum.Hard).ToList();

            if (easyPool.Count < easyCount || mediumPool.Count < mediumCount || hardPool.Count < hardCount)
                return null;

            var selected = new List<Question>();
            for (var attempt = 0; attempt < 30; attempt++)
            {
                selected.Clear();
                var shuffledEasy = easyPool.OrderBy(_ => Random.Shared.Next()).ToList();
                var shuffledMedium = mediumPool.OrderBy(_ => Random.Shared.Next()).ToList();
                var shuffledHard = hardPool.OrderBy(_ => Random.Shared.Next()).ToList();

                if (TrySelectRecursive(shuffledEasy, shuffledMedium, shuffledHard, 0, 0, 0, easyCount, mediumCount, hardCount, requiredTotalPoints, selected))
                    return selected.OrderBy(_ => Random.Shared.Next()).ToList();
            }

            selected.Clear();
            if (TrySelectRecursive(easyPool, mediumPool, hardPool, 0, 0, 0, easyCount, mediumCount, hardCount, requiredTotalPoints, selected))
                return selected.OrderBy(_ => Random.Shared.Next()).ToList();

            return null;
        }

        public static string? ValidatePoolAvailability(
            IReadOnlyList<Question> pool,
            int easyCount,
            int mediumCount,
            int hardCount,
            int questionCount,
            int requiredTotalPoints)
        {
            if (easyCount + mediumCount + hardCount != questionCount)
                return "Tổng số câu Dễ + Trung bình + Khó phải bằng số câu hỏi trong đề.";

            var easyAvailable = pool.Count(q => q.DifficultyLevel == (int)DifficultyLevelEnum.Easy);
            var mediumAvailable = pool.Count(q => q.DifficultyLevel == (int)DifficultyLevelEnum.Medium);
            var hardAvailable = pool.Count(q => q.DifficultyLevel == (int)DifficultyLevelEnum.Hard);

            if (easyAvailable < easyCount)
                return $"Không đủ câu Dễ (cần {easyCount}, có {easyAvailable}).";
            if (mediumAvailable < mediumCount)
                return $"Không đủ câu Trung bình (cần {mediumCount}, có {mediumAvailable}).";
            if (hardAvailable < hardCount)
                return $"Không đủ câu Khó (cần {hardCount}, có {hardAvailable}).";

            if (TrySelectRandomQuestions(pool, easyCount, mediumCount, hardCount, requiredTotalPoints) == null)
                return $"Không thể ghép {questionCount} câu với tổng điểm đúng {requiredTotalPoints} từ ngân hàng đã chọn.";

            return null;
        }

        private static bool TrySelectRecursive(
            IReadOnlyList<Question> easy,
            IReadOnlyList<Question> medium,
            IReadOnlyList<Question> hard,
            int easyIdx,
            int mediumIdx,
            int hardIdx,
            int needEasy,
            int needMedium,
            int needHard,
            int targetPoints,
            List<Question> current)
        {
            if (needEasy == 0 && needMedium == 0 && needHard == 0)
                return current.Sum(q => q.Points) == targetPoints;

            var currentSum = current.Sum(q => q.Points);
            var remainingCount = needEasy + needMedium + needHard;
            var remainingPoints = targetPoints - currentSum;

            if (remainingCount == 0)
                return remainingPoints == 0;

            if (needEasy > 0)
            {
                if (easy.Count - easyIdx < needEasy)
                    return false;

                for (var i = easyIdx; i <= easy.Count - needEasy; i++)
                {
                    if (!CanReachTarget(current, easy[i], remainingCount, remainingPoints, easy, medium, hard, needEasy - 1, needMedium, needHard, i + 1, mediumIdx, hardIdx))
                        continue;

                    current.Add(easy[i]);
                    if (TrySelectRecursive(easy, medium, hard, i + 1, mediumIdx, hardIdx, needEasy - 1, needMedium, needHard, targetPoints, current))
                        return true;
                    current.RemoveAt(current.Count - 1);
                }
                return false;
            }

            if (needMedium > 0)
            {
                if (medium.Count - mediumIdx < needMedium)
                    return false;

                for (var i = mediumIdx; i <= medium.Count - needMedium; i++)
                {
                    if (!CanReachTarget(current, medium[i], remainingCount, remainingPoints, easy, medium, hard, 0, needMedium - 1, needHard, easyIdx, i + 1, hardIdx))
                        continue;

                    current.Add(medium[i]);
                    if (TrySelectRecursive(easy, medium, hard, easyIdx, i + 1, hardIdx, 0, needMedium - 1, needHard, targetPoints, current))
                        return true;
                    current.RemoveAt(current.Count - 1);
                }
                return false;
            }

            if (hard.Count - hardIdx < needHard)
                return false;

            for (var i = hardIdx; i <= hard.Count - needHard; i++)
            {
                if (!CanReachTarget(current, hard[i], remainingCount, remainingPoints, easy, medium, hard, 0, 0, needHard - 1, easyIdx, mediumIdx, i + 1))
                    continue;

                current.Add(hard[i]);
                if (TrySelectRecursive(easy, medium, hard, easyIdx, mediumIdx, i + 1, 0, 0, needHard - 1, targetPoints, current))
                    return true;
                current.RemoveAt(current.Count - 1);
            }

            return false;
        }

        private static bool CanReachTarget(
            List<Question> current,
            Question candidate,
            int remainingCount,
            int remainingPoints,
            IReadOnlyList<Question> easy,
            IReadOnlyList<Question> medium,
            IReadOnlyList<Question> hard,
            int needEasy,
            int needMedium,
            int needHard,
            int easyIdx,
            int mediumIdx,
            int hardIdx)
        {
            var newSum = current.Sum(q => q.Points) + candidate.Points;
            var newRemaining = remainingPoints - candidate.Points;
            var newCount = remainingCount - 1;

            if (newRemaining < 0)
                return false;

            var availablePoints = new List<int>();
            availablePoints.AddRange(easy.Skip(easyIdx).Take(needEasy).Select(q => q.Points));
            availablePoints.AddRange(medium.Skip(mediumIdx).Take(needMedium).Select(q => q.Points));
            availablePoints.AddRange(hard.Skip(hardIdx).Take(needHard).Select(q => q.Points));

            if (availablePoints.Count != newCount)
            {
                availablePoints.Clear();
                if (needEasy > 0) availablePoints.AddRange(easy.Skip(easyIdx).Select(q => q.Points));
                if (needMedium > 0) availablePoints.AddRange(medium.Skip(mediumIdx).Select(q => q.Points));
                if (needHard > 0) availablePoints.AddRange(hard.Skip(hardIdx).Select(q => q.Points));
            }

            if (newCount == 0)
                return newRemaining == 0;

            var sorted = availablePoints.OrderBy(x => x).ToList();
            var minPossible = sorted.Take(newCount).Sum();
            var maxPossible = sorted.Skip(Math.Max(0, sorted.Count - newCount)).Sum();
            return newRemaining >= minPossible && newRemaining <= maxPossible;
        }
    }
}
