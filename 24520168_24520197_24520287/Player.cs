using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
<<<<<<< HEAD
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
=======
using System.Media;
>>>>>>> Binh

namespace _24520168_24520197_24520287
{

    public enum PlayerState
    {
        Idle,
        Run,
        Jump,
        Fall,
        Shoot,   // Bắn cung
        Hurt,    // Bị thương
        Death,
    }

    public class Player : GameObject
    {
        private const float Gravity = 0.5f; //Trọng lực tác động lên nhân vật
        private const float MoveSpeed = 5f; //Tốc độ di chuyển ngang
        private const float JumpForce = -12f; //Lực nhảy của nhân vật
        private const float MaxFallSpeed = 15f; //Tốc độ rơi tối đa

        private Dictionary<Keys, bool> keyStates; //Trạng thái phím bấm
        private bool canJump; //Kiểm tra xem nhân vật có thể nhảy hay không

        //Hướng quay mặt và bắn
        private int facing = 1; //1: phải, -1: trái/
        private float fireCooldown = 0f;
<<<<<<< HEAD
        private const float FireRate = 0.2f;

=======
        private const float FireRate = 0.35f;
        //Âm thanh player hurt
        private static SoundPlayer hitPlayer;
>>>>>>> Binh
        //Máu
        public int MaxHealth { get; private set; }
        public int Health { get; private set; }

        public event EventHandler PlayerDied;

<<<<<<< HEAD
        //Animation
        private Bitmap spriteSheet; // Dùng Bitmap để có thể cắt (crop)
        private int currentFrame = 0;
        private float frameTimer = 0;
        private bool isHurt = false;
        private bool isShooting = false; // Biến để kích hoạt/quản lý animation Bắn
        private const int FrameWidth = 64; // Kích thước 1 frame trên sheet
        private const int FrameHeight = 64;
        private const int FramesPerRow = 11;

        public struct Animation
        {
            public int StartFrame;  // Chỉ số khung hình đầu tiên trên sprite sheet
            public int FrameCount;  // Tổng số khung hình
            public float FrameDuration; // Thời gian hiển thị mỗi khung hình (ví dụ: 0.1f)
        }
        private PlayerState currentState = PlayerState.Idle;
        private Dictionary<PlayerState, Animation> animations;
=======
        //Khởi tạo âm thanh player hurt
        static Player()
        {
            try
            {
                hitPlayer = new SoundPlayer(Application.StartupPath + "\\resources\\Player Hurt.wav");
                hitPlayer.Load(); //tải âm thanh khi nhận sát thương
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error loading player sound: " + ex.Message);
                hitPlayer = null;
            }
        }
>>>>>>> Binh

        public Player(float x, float y)
        {
            X = x;
            Y = y;
            VelocityX = 0;
            VelocityY = 0;
            IsOnGround = false;
            keyStates = new Dictionary<Keys, bool>
            {
                { Keys.Left, false },//Phím di chuyển ngang
                { Keys.Right, false },//Phím di chuyển ngang
                { Keys.Up, false }, //Phím nhảy
            };

            MaxHealth = 100;
            Health = MaxHealth;

            Width = FrameWidth; // 44
            Height = FrameHeight; // 64

            try
            {
                spriteSheet = new Bitmap("Assets\\Player\\GandalfHardcoreArchersheet.png");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể tải Sprite Sheet: {ex.Message}");
            }

            animations = new Dictionary<PlayerState, Animation>
            {
                // HÀNG 0: IDLE (4 frames đầu tiên)
                { PlayerState.Idle, new Animation { StartFrame = 0, FrameCount = 4, FrameDuration = 0.15f } },
        
                // HÀNG 1: SHOOT (8 frames đầu tiên)
                { PlayerState.Shoot, new Animation { StartFrame = 11, FrameCount = 8, FrameDuration = 0.02f } }, 
        
                // HÀNG 2: RUN (4 frames đầu tiên)
                { PlayerState.Run, new Animation { StartFrame = 22, FrameCount = 4, FrameDuration = 0.10f } },
        
                // HÀNG 3: HURT (4 frames đầu tiên)
                { PlayerState.Hurt, new Animation { StartFrame = 33, FrameCount = 4, FrameDuration = 0.1f } },

                // HÀNG 4: DEATH (4 frames cuối, bắt đầu từ index 51)
                { PlayerState.Death, new Animation { StartFrame = 51, FrameCount = 4, FrameDuration = 0.2f } },

                // JUMP/FALL: Vẫn dùng frame tĩnh từ Idle (index 2)
                { PlayerState.Jump, new Animation { StartFrame = 2, FrameCount = 1, FrameDuration = 1f } },
                { PlayerState.Fall, new Animation { StartFrame = 2, FrameCount = 1, FrameDuration = 1f } }
            };
        }
        public void SetKeyState(Keys key, bool isPressed)
        {
            if (keyStates.ContainsKey(key))
            {
                keyStates[key] = isPressed;
            }
        }

        public void TakeDamage(int amount)
        {
            if (Health <= 0) return;

            Health -= amount;
<<<<<<< HEAD
            if (Health <= 0)
=======
            if(hitPlayer != null && !Form1.SfxMuted) // âm thanh player bị thương
            {
                hitPlayer.Play();
            }

            if (Health < 0)
>>>>>>> Binh
            {
                Health = 0;
                OnPlayerDied();
            }
            else
            {
                isHurt = true;
                currentFrame = 0;
                frameTimer = 0;
            }    
        }

        protected virtual void OnPlayerDied()
        {
            PlayerDied?.Invoke(this, EventArgs.Empty);
        }

        public Projectile Fire()
        {
            if (fireCooldown > 0) return null;
            float px = X + Width / 2f;
            float py = Y + Height / 2f;
            float speed = 10f; 
            float vx = speed * facing;
            float vy = 0;
            int damage = 20;
            var proj = new Projectile(px, py + 6f, vx, vy, 5f, damage, this);

            fireCooldown = FireRate;
            isShooting = true;
            return proj;
        }

        public override void Draw(Graphics g)
        {
            if (spriteSheet != null && animations.ContainsKey(currentState))
            {
                Animation currentAnim = animations[currentState];

                // 1. Tính toán index frame tuyệt đối (Giữ nguyên)
                int absoluteFrameIndex = currentAnim.StartFrame + currentFrame;

                // 2. Tính toán Cột (Col) và Hàng (Row) của frame trên sheet
                int frameCol = absoluteFrameIndex % FramesPerRow;
                int frameRow = absoluteFrameIndex / FramesPerRow;

                // 3. Tính toán sourceX và sourceY dựa trên Cột và Hàng
                int sourceX = frameCol * FrameWidth;
                int sourceY = frameRow * FrameHeight;

                // 4. Tạo srcRect với X và Y chính xác
                Rectangle srcRect = new Rectangle(sourceX, sourceY, FrameWidth, FrameHeight); // <-- SỬA Ở ĐÂY

                // 5. Vẽ (Phần còn lại giữ nguyên)
                Rectangle destRect = new Rectangle((int)X, (int)Y, (int)Width, (int)Height);

                if (facing == 1) // Hướng phải
                {
                    g.DrawImage(spriteSheet, destRect, srcRect, GraphicsUnit.Pixel);
                }
                else // Hướng trái
                {
                    Rectangle flippedDestRect = new Rectangle((int)X + (int)Width, (int)Y, -(int)Width, (int)Height);
                    g.DrawImage(spriteSheet, flippedDestRect, srcRect, GraphicsUnit.Pixel);
                }
            }
            else
            {
                // Dự phòng: Vẽ hình chữ nhật nếu không có sprite sheet
                g.FillRectangle(Brushes.Blue, X, Y, Width, Height);
            }

            //Vẽ thanh máu cho player nha
            float barW = Width;
            float barH = 6;
            float barX = X;
            float barY = Y - barH - 2;
            g.FillRectangle(Brushes.Red, barX, barY, barW, barH);
            float ratio = (MaxHealth > 0) ? (float)Health / MaxHealth : 0;
            g.FillRectangle(Brushes.LimeGreen, barX, barY, barW * ratio, barH);
            g.DrawRectangle(Pens.Black, barX, barY, barW, barH);
        }

        public override void Update(List<Platform> platforms)
        {
            if (fireCooldown > 0f)
                fireCooldown -= 0.016f;

            VelocityX = 0;
            if(keyStates[Keys.Left])
            {
                VelocityX = -MoveSpeed;
                facing = -1;
            }
            if(keyStates[Keys.Right])
            {
                VelocityX = MoveSpeed;
                facing = 1;
            }
            if(keyStates[Keys.Up] && IsOnGround && canJump)
            {
                VelocityY = JumpForce;
                IsOnGround = false;
                canJump = false;
            }
            if(!keyStates[Keys.Up])
            {
                canJump = true;
            }
            VelocityY += Gravity;
            if(VelocityY > MaxFallSpeed)
            {
                VelocityY = MaxFallSpeed;
            }
            X += VelocityX;
            HandleHorizontalCollisions(platforms);
            Y += VelocityY;
            HandleVerticalCollisions(platforms);

            PlayerState newState = currentState;

            // 1. Ưu tiên cao nhất: CHẾT
            if (Health <= 0)
            {
                newState = PlayerState.Death;
            }
            // 2. Ưu tiên: BỊ THƯƠNG (Thêm vào đây)
            else if (isHurt)
            {
                newState = PlayerState.Hurt;
            }
            // 3. Ưu tiên: BẮN CUNG
            else if (isShooting)
            {
                newState = PlayerState.Shoot;
            }
            // 4. Trạng thái Vật lý/Di chuyển (Khi rảnh rỗi)
            else
            {
                if (!IsOnGround) // Trên không
                {
                    newState = (VelocityY < 0) ? PlayerState.Jump : PlayerState.Fall;
                }
                else // Trên đất
                {
                    newState = (Math.Abs(VelocityX) > 0.1f) ? PlayerState.Run : PlayerState.Idle;
                }
            }

            // 3. Xử lý Chuyển đổi Trạng thái và Reset Frame (Giữ nguyên)
            if (newState != currentState)
            {
                currentState = newState;
                currentFrame = 0;
                frameTimer = 0;
            }

            // 4. Cập nhật Frame Animation
            Animation currentAnim = animations[currentState];

            if (currentAnim.FrameCount > 1)
            {
                frameTimer += 0.016f;
                if (frameTimer >= currentAnim.FrameDuration)
                {
                    currentFrame = (currentFrame + 1);
                    frameTimer = 0;

                    // KIỂM TRA KẾT THÚC ANIMATION KHÔNG LẶP
                    if (currentState == PlayerState.Shoot)
                    {
                        if (currentFrame >= currentAnim.FrameCount)
                        {
                            currentFrame = 0;
                            isShooting = false;
                        }
                    }
                    // THÊM XỬ LÝ CHO HURT
                    else if (currentState == PlayerState.Hurt)
                    {
                        if (currentFrame >= currentAnim.FrameCount)
                        {
                            currentFrame = 0;
                            isHurt = false; // Tắt cờ Hurt để quay về Idle/Run
                        }
                    }
                    else if (currentState == PlayerState.Death)
                    {
                        if (currentFrame >= currentAnim.FrameCount)
                        {
                            currentFrame = currentAnim.FrameCount - 1;
                            frameTimer = currentAnim.FrameDuration;
                        }
                    }
                    else // Các animation lặp (Idle, Run)
                    {
                        currentFrame = currentFrame % currentAnim.FrameCount;
                    }
                }
            }

            if (X < 0)
            {
                X = 0;
                VelocityX = 0;
            }

            // 3. Ranh giới trên (Không cho nhảy quá Y < 0)
            if (Y < 0)
            {
                Y = 0;
                VelocityY = 0;
            }

            // 1. Ranh giới rơi (Death Plane)
            if (Y > 1000 && Health > 0)
            {
                TakeDamage(MaxHealth);
            }

        }
        private void HandleHorizontalCollisions(List<Platform> platforms)
        {
            RectangleF playerBounds = GetBounds();
            foreach (var platform in platforms)
            {
                if(playerBounds.IntersectsWith(platform.GetBounds()))
                {
                    if(VelocityX > 0)
                    {
                        X = platform.X - Width;
                    }
                    else if(VelocityX < 0)
                    {
                        X = platform.X + platform.Width;
                    }
                    VelocityX = 0;
                }
            }
        }
        private void HandleVerticalCollisions(List<Platform> platforms)
        {
            RectangleF playerBounds = GetBounds();
            IsOnGround = false;
            foreach(var platform in platforms)
            {
                if(playerBounds.IntersectsWith(platform.GetBounds()))
                {
                    if(VelocityY > 0) //Đang rơi xuống
                    {
                        Y = platform.Y - Height;
                        IsOnGround = true;
                    }
                    else if(VelocityY < 0) //Đang nhảy lên
                    {
                        Y = platform.Y + platform.Height;
                    }
                    VelocityY = 0;
                }
            }
        }
    }
}
