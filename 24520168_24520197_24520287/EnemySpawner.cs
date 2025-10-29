using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _24520168_24520197_24520287
{
    public class EnemySpawner
    {
        private Timer spawnTimer;
        private Random random;
        private Player player;
        private List<Enemy> enemies;
        private List<Platform> platforms;
        private int maxEnemies = 5;

        public EnemySpawner(Player player, List<Enemy> enemyList, List<Platform> platformList)
        {
            this.player = player;
            this.enemies = enemyList;
            this.platforms = platformList;
            random = new Random();
            spawnTimer = new Timer();
            spawnTimer.Interval = 3000; // Spawn mỗi 3 giây
            spawnTimer.Tick += TrySpawnEnemy;
        }
        private void TrySpawnEnemy(object sender, EventArgs e)
        {
            // Giới hạn số lượng enemy
            enemies.RemoveAll(enemy => enemy.isDead);

            if (enemies.Count >= maxEnemies)
                return;

            // Chọn platform ngẫu nhiên để spawn (không spawn trên platform của player)
            var validPlatforms = platforms.Where(p =>
                Math.Abs(p.X - player.X) > 200).ToList();

            if (validPlatforms.Count > 0)
            {
                var platform = validPlatforms[random.Next(validPlatforms.Count)];
                float spawnX = platform.X + random.Next((int)platform.Width - 40);
                float spawnY = platform.Y - 60; // Spawn trên platform

                enemies.Add(new Enemy(spawnX, spawnY, player));
            }
        }

        public void Start() => spawnTimer.Start();
        public void Stop() => spawnTimer.Stop();
    }
}

