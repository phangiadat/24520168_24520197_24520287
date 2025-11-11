using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace _24520168_24520197_24520287
{
    public enum PlayerState
    {
        Idle,
        Run,
        Jump,
        Fall,
        Shoot,
        Hurt,
        Death,
    }

    public class Player : GameObject
    {
        private const float Gravity = 0.5f;     // Trọng lực
        private const float MoveSpeed = 5f;     // Tốc độ ngang
        private const float JumpForce = -12f;   // Lực nhảy
        private const float MaxFallSpeed = 15f; // Tốc độ rơi tối đa

        private readonly Dictionary<Keys, bool> keyStates; // Trạng thái phím
        private bool canJump = true; // Cho phép nhảy ngay từ đầu

        // Hướng và bắn
        private int facing = 1; // 1: phải, -1: trái
        private float fireCooldown = 0f;
        private const float FireRate = 0.35f;

        // Âm thanh player bị thương
        private static SoundPlayer hitPlayer;

        // Máu
        public int MaxHealth { get; private set; }
        public int Health { get; private set; }
        public event EventHandler PlayerDied;

        // Animation / sprite sheet
        private Bitmap spriteSheet;
        private int currentFrame = 0;
