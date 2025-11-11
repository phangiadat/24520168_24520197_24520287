using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace _24520168_24520197_24520287
{
    public class EnemySpawner : IDisposable
    {
        private Timer spawnTimer;
        private Random random;
        private Player player;
        private List<Enemy> enemies;
        private List<Platform> platforms;
        private int maxEnemies = 8;

        public EnemySpawner(Player player, List<Enemy> enemyList, List<Platform> platformList)
        {
            this.player = player;
            this.enemies = enemyList;
            this.platforms = platformList;
            random = new Random();
            spawnTimer = new Timer();
            spawnTimer.Interval = 3000; // Spawn every 3 seconds
            spawnTimer.Tick += TrySpawnEnemy;
        }


        private void TrySpawnEnemy(object sender, EventArgs e)
        {
            // Clean dead enemies
            enemies.RemoveAll(enemy => enemy.isDead); 

            if (enemies.Count >= maxEnemies)
                return;

            // Choose a platform not too close to the player
            var validPlatforms = platforms.Where(p => Math.Abs(p.X - player.X) > 200).ToList();
            if (validPlatforms.Count == 0) return;

            var platform = validPlatforms[random.Next(validPlatforms.Count)];

            // Ensure random.Next argument is positive
            int maxOffset = Math.Max(1, (int)platform.Width - 40);
            int offset = random.Next(0, maxOffset);
            float spawnX = platform.X + offset;
            float spawnY = platform.Y - 60; // spawn slightly above platform

            enemies.Add(new Enemy(spawnX, spawnY, player));
        }

        public void Start() => spawnTimer.Start();
        public void Stop() => spawnTimer.Stop();

        public void Dispose()
        {
            spawnTimer?.Stop();
            spawnTimer?.Dispose();
            spawnTimer = null;
        }
    }
}

