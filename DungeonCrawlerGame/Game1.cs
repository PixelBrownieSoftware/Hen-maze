using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Solarheart;
using BrownieEngine;
using System.Collections.Generic;
using System;

namespace DungeonCrawlerGame
{

    /// <summary>
    /// A collection of 20x20 tiled maps
    /// </summary>
    public struct s_dungeonMap
    {
        public s_map[,] area;
        public Point mapSize;
        public s_dungeonMap(s_map[,] area, Point mapSize) {
            this.area = area;
            this.mapSize = mapSize;
        }
    }
    public struct s_dungeonLayout
    {
        public s_map[] leftTop; //  <^
        public s_map[] rightTop; //  ^>
        public s_map[] bottomTop; //  ^v

        public s_map[] rightBottom; //  v>
        public s_map[] leftBottom; //  <v

        public s_map[] leftRight; //  <>
        public Point layoutMaxSize;
        public Point layoutMinSize;

        public s_dungeonLayout(s_map[] leftTop, s_map[] rightTop, s_map[] bottomTop, s_map[] rightBottom, s_map[] leftBottom, s_map[] leftRight, Point layoutMaxSize, Point layoutMinSize)
        {
            this.leftTop = leftTop;
            this.rightTop = rightTop;
            this.bottomTop = bottomTop;
            this.rightBottom = rightBottom;
            this.leftBottom = leftBottom;
            this.leftRight = leftRight;
            this.layoutMaxSize = layoutMaxSize;
            this.layoutMinSize = layoutMinSize;
        }
    }

    public class o_player : s_object
    {
        public int health = 4;
        public bool noClip = false;
        public bool isRed = true;
        const float defaultSpeed = 2.45f;
        float speed = 4f;
        public Vector2 slopeStepPos;
        public Vector2 tilePos;

        Vector2 lastFallPos;

        float gravity = 0;

        public float zPos;
        bool isGrounded = true;

        float conveyorDelay = 0.5f; //Time delay of each step on the conveyor belt

        public ushort slopeTil; Vector2 velDir; Vector2 directionAnim;

        public ushort slidingPT; //These are the points that the char checks when silding

        public bool control = true;

        public Vector2 offset;
        public s_animhandler animHandler;

        public override void Start()
        {
            speed = defaultSpeed;
            collisionBox = new Rectangle(0, 0, 20, 20);
            offset = new Vector2(collisionBox.Width/2, (collisionBox.Height/2) - 1);

            renderer.SetSprite("player",4);
            animHandler = new s_animhandler();
            {
                s_anim an = new s_anim("idle_u");
                an.AddAnimation(2, 0.1f);
                animHandler.AddAnimation(an);
            };
            {
                s_anim an = new s_anim("idle_r");
                an.AddAnimation(1, 0.1f);
                an.spriteFx = SpriteEffects.FlipHorizontally;
                animHandler.AddAnimation(an);
            };
            {
                s_anim an = new s_anim("idle_l");
                an.AddAnimation(1, 0.1f);
                animHandler.AddAnimation(an);
            };
            {
                s_anim an = new s_anim("idle_d");
                an.AddAnimation(0, 0.1f);
                animHandler.AddAnimation(an);
            };

            {
                s_anim an = new s_anim("walk_u");
                an.AddAnimation(2, 0.05f);
                an.AddAnimation(6, 0.05f);
                //an.AddAnimation(4, 0f, s_anim.ANIM_TYPE.SOUND);
                an.AddAnimation(2, 0.05f);
                an.AddAnimation(9, 0.05f);
                animHandler.AddAnimation(an);
            };
            {
                s_anim an = new s_anim("walk_d");
                an.AddAnimation(0, 0.05f);
                an.AddAnimation(4, 0.05f);
                //an.AddAnimation(4, 0f, s_anim.ANIM_TYPE.SOUND);
                an.AddAnimation(0, 0.05f);
                an.AddAnimation(7, 0.05f);
                animHandler.AddAnimation(an);
            };
            {
                s_anim an = new s_anim("walk_r");
                an.AddAnimation(1, 0.05f);
                //an.AddAnimation(4, 0f, s_anim.ANIM_TYPE.SOUND);
                an.AddAnimation(5, 0.05f);
                animHandler.AddAnimation(an);
            };
            {
                s_anim an = new s_anim("walk_l");
                an.spriteFx = SpriteEffects.FlipHorizontally;
                an.AddAnimation(1, 0.05f);
                //an.AddAnimation(4, 0, s_anim.ANIM_TYPE.SOUND);
                an.AddAnimation(5, 0.05f);
                animHandler.AddAnimation(an);
            };

            {
                s_anim an = new s_anim("jump_l");
                an.AddAnimation(8, 0.1f);
                animHandler.AddAnimation(an);
            };
            {
                s_anim an = new s_anim("jump_d");
                an.AddAnimation(3, 0.1f);
                an.AddAnimation(9, 0.1f);
                animHandler.AddAnimation(an);
            };
            {
                s_anim an = new s_anim("jump_u");
                an.AddAnimation(3, 0.1f);
                an.AddAnimation(9, 0.1f);
                animHandler.AddAnimation(an);
            };
        }

        public void GoUpSlope()
        {
            Vector2 v = Game1.game.getTruncLevel(position + offset) * 20;
            float ypos = -1f * (position.X % 20) + v.Y;
            position.Y = ypos;
            if (ypos < v.Y + 19)
                position.Y -= 1;
            slopeStepPos = new Vector2(position.X, ypos);
        }
        
        public bool CheckIfCharacterInTile(string layername)
        {
            ForceCollisionBoxUpdate();
            Vector2 f = collisionBox.Location.ToVector2();
            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    ushort tilePix = Game1.GetDungeonTile(f + new Vector2(x, y));
                    if (Game1.game.IsSameLayer(tilePix, layername))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckIfCharacterInTile()
        {
            ForceCollisionBoxUpdate();
            Vector2 f = collisionBox.Location.ToVector2();
            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    ushort tilePix = Game1.GetDungeonTile(f + new Vector2(x, y));
                    if (tilePix == 5)
                    {
                        if (isRed)
                        {
                            return true;
                        }
                    }
                    if (tilePix == 4)
                    {
                        if (!isRed)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void CollisionDetectionY(string layer)
        {
            ForceCollisionBoxUpdate();
            Vector2 f = collisionBox.Location.ToVector2();
            ushort topR = Game1.GetDungeonTile(f + new Vector2(0, -1));
            ushort topL = Game1.GetDungeonTile(f + new Vector2(collisionBox.Width - 1, -1));

            ushort bottomR = Game1.GetDungeonTile(f + new Vector2(0, collisionBox.Height));
            ushort bottomL = Game1.GetDungeonTile(f + new Vector2(collisionBox.Width - 1, collisionBox.Height));

            ushort midR = Game1.GetDungeonTile(f + new Vector2(-1, collisionBox.Height / 2));
            ushort midL = Game1.GetDungeonTile(f + new Vector2(collisionBox.Width + 1, collisionBox.Height / 2));

            ushort leftT = Game1.GetDungeonTile(f + new Vector2(collisionBox.Width, collisionBox.Height - 2));
            ushort leftB = Game1.GetDungeonTile(f + new Vector2(collisionBox.Width, 0));

            ushort rightT = Game1.GetDungeonTile(f + new Vector2(-1, collisionBox.Height - 2));
            ushort rightB = Game1.GetDungeonTile(f + new Vector2(-1, 1));

            if (Game1.game.IsSameLayer(bottomL, layer) || Game1.game.IsSameLayer(bottomR, layer))
            {

                if (velocity.Y > 0)
                {
                    Vector2 v = Game1.game.getTruncLevel(centre + new Vector2(0, collisionBox.Height));
                    velocity.Y = 0;
                    v.Y *= Game1.game.level.tileSizeY;
                    position.Y = v.Y - Game1.game.level.tileSizeY; //(collisionBox.Height - Game1.game.level.tileSizeY);
                }
            }
            if (Game1.game.IsSameLayer(topR, layer) || Game1.game.IsSameLayer(topL, layer))
            {
                if (velocity.Y < 0)
                {
                    velocity.Y = 0;
                    //Vector2 v = Game1.game.getTruncLevel(collisionBox.Location.ToVector2() - new Vector2(0, collisionBox.Height/2)) - new Vector2(0, height);
                    Vector2 v = Game1.game.getTruncLevel(position - new Vector2(0, 1));// + new Vector2(0, height/2);
                    v.Y *= Game1.game.level.tileSizeY;
                    //position.Y = v.Y; + collisionBox.Height
                    position.Y = v.Y + Game1.game.level.tileSizeY;
                }
            }
        }
        public void CollisionDetectionX(string layer)
        {
            ForceCollisionBoxUpdate();
            Vector2 f = collisionBox.Location.ToVector2();
            ushort topR = Game1.GetDungeonTile(f + new Vector2(0, -1));
            ushort topL = Game1.GetDungeonTile(f + new Vector2(collisionBox.Width - 1, -1));

            ushort bottomR = Game1.GetDungeonTile(f + new Vector2(0, collisionBox.Height));
            ushort bottomL = Game1.GetDungeonTile(f + new Vector2(collisionBox.Width - 1, collisionBox.Height));

            ushort midR = Game1.GetDungeonTile(f + new Vector2(-1, collisionBox.Height / 2));
            ushort midL = Game1.GetDungeonTile(f + new Vector2(collisionBox.Width + 1, collisionBox.Height / 2));

            ushort leftT = Game1.GetDungeonTile(f + new Vector2(collisionBox.Width, collisionBox.Height - 2));
            ushort leftB = Game1.GetDungeonTile(f + new Vector2(collisionBox.Width, 0));

            ushort rightT = Game1.GetDungeonTile(f + new Vector2(-1, collisionBox.Height - 2));
            ushort rightB = Game1.GetDungeonTile(f + new Vector2(-1, 1));


            if (Game1.game.IsSameLayer(leftT, layer) || Game1.game.IsSameLayer(leftB, layer) || Game1.game.IsSameLayer(midL, layer))
            {
                if (velocity.X > 0)
                {
                    velocity.X = 0;
                    Vector2 v = Game1.game.getTruncLevel(centre + new Vector2(20, 0)) + new Vector2(-1, 0);
                    v.X *= Game1.game.level.tileSizeX;
                    position.X = v.X;
                    return;
                }
            }
            if (Game1.game.IsSameLayer(rightT, layer) || Game1.game.IsSameLayer(rightB, layer) || Game1.game.IsSameLayer(midR, layer))
            {
                if (velocity.X < 0)
                {
                    velocity.X = 0;
                    Vector2 v = Game1.game.getTruncLevel(centre - new Vector2(20, 0)) + new Vector2(1, 0);
                    v.X *= Game1.game.level.tileSizeX;
                    position.X = v.X;
                    return;
                }
            }
        }

        public override void Update(GameTime gametime)
        {
            tilePos = Game1.game.getTruncLevel(position + offset) * 20;

            slopeStepPos = position - offset + (velDir * 10);


            if (isGrounded)
                if (Game1.game.IsSameLayer(slidingPT, "ditch"))
                {
                    Game1.g1.BackToMenu();
                }

            if (Game1.game.IsSameLayer(slidingPT, "door"))
            {
                Game1.g1.NextMap();
            }

            if (zPos < 0)
            {
                gravity += Game1.deltaTime * 2;
                zPos += gravity;
                if (velDir.X == -1)
                    animHandler.SetAnimation("jump_r", true);
                if (velDir.X == 1)
                    animHandler.SetAnimation("jump_l", true);
            }
            else
            {
                zPos = 0;
                isGrounded = true;
            }
            if (control)
            {
                if (isGrounded)
                {
                    speed = defaultSpeed;
                    if (Game1.KeyPressed(Keys.Space))
                    {
                        isGrounded = false;
                        gravity = -1;
                        zPos = -1f;
                    }

                    if (Game1.KeyPressedDown(Keys.E) && CheckIfCharacterInTile())
                    {
                        Game1.game.PlaySound(5);
                        isRed = !isRed ? true : false;
                    }
                }
                if (isGrounded)
                {
                    if (Game1.KeyPressed(Keys.Right) &&
                        !Game1.KeyPressed(Keys.Down) &&
                        !Game1.KeyPressed(Keys.Up))
                    {
                        directionAnim.X = 1;
                        directionAnim.Y = 0;
                        animHandler.SetAnimation("walk_r", true);
                    }
                    if (Game1.KeyPressed(Keys.Up)
                        || Game1.KeyPressed(Keys.Up) && Game1.KeyPressed(Keys.Left)
                        || Game1.KeyPressed(Keys.Up) && Game1.KeyPressed(Keys.Right))
                    {
                        directionAnim.Y = 1;
                        if (Game1.KeyPressed(Keys.Up) && !Game1.KeyPressed(Keys.Left)
                        || Game1.KeyPressed(Keys.Up) && !Game1.KeyPressed(Keys.Right))
                            directionAnim.X = 0;
                        animHandler.SetAnimation("walk_u", true);
                    }
                    if (Game1.KeyPressed(Keys.Down)
                        || Game1.KeyPressed(Keys.Down) && Game1.KeyPressed(Keys.Left)
                        || Game1.KeyPressed(Keys.Down) && Game1.KeyPressed(Keys.Right))
                    {
                        directionAnim.Y = -1;
                        if (Game1.KeyPressed(Keys.Down) && !Game1.KeyPressed(Keys.Left)
                        || Game1.KeyPressed(Keys.Down) && !Game1.KeyPressed(Keys.Right))
                            directionAnim.X = 0;
                        animHandler.SetAnimation("walk_d", true);
                    } 
                    if (Game1.KeyPressed(Keys.Left) &&
                        !Game1.KeyPressed(Keys.Down) &&
                        !Game1.KeyPressed(Keys.Up))
                    {
                        directionAnim.X = -1;
                        directionAnim.Y = 0;
                        animHandler.SetAnimation("walk_l", true);
                    }
                    if (!Game1.KeyPressed(Keys.Right) &&
                        !Game1.KeyPressed(Keys.Left) &&
                        !Game1.KeyPressed(Keys.Up) &&
                        !Game1.KeyPressed(Keys.Down))
                    {
                        if (directionAnim.Y == 0 && directionAnim.X == 1)
                        {
                            animHandler.SetAnimation("idle_l", true);
                        }
                        if (directionAnim.Y == 0 && directionAnim.X == -1)
                        {
                            animHandler.SetAnimation("idle_r", true);
                        }
                        if (directionAnim.Y == 1)
                        {
                            animHandler.SetAnimation("idle_u", true);
                        }
                        if (directionAnim.Y == -1)
                        {
                            animHandler.SetAnimation("idle_d", true);
                        }
                    }
                    speed = defaultSpeed;
                    if (Game1.KeyPressed(Keys.Space))
                    {
                        isGrounded = false;
                        gravity = -1;
                        zPos = -1f;
                    }

                    if (Game1.KeyPressedDown(Keys.E) && !CheckIfCharacterInTile())
                    {
                        Game1.game.PlaySound(5);
                        isRed = !isRed ? true : false;
                    }
                }
                else {
                    speed = defaultSpeed / 2;
                }
                if (Game1.KeyPressed(Keys.S))
                {
                    position = tilePos - offset;
                }

                if (Game1.KeyPressed(Keys.Right))
                {
                    velDir = new Vector2(1, 0);
                    velocity.X = speed;
                }
                if (Game1.KeyPressed(Keys.Up))
                {
                    velDir = new Vector2(0, -1);
                    velocity.Y = -speed;
                }
                if (Game1.KeyPressed(Keys.Down))
                {
                    velDir = new Vector2(0, 1);
                    velocity.Y = speed;
                }
                if (Game1.KeyPressed(Keys.Left))
                {
                    velDir = new Vector2(-1, 0);
                    velocity.X = -speed;
                }
                if (!Game1.KeyPressed(Keys.Left) && !Game1.KeyPressed(Keys.Right))
                {
                    velocity.X = 0;
                }

                if (!Game1.KeyPressed(Keys.Up) && !Game1.KeyPressed(Keys.Down))
                    velocity.Y = 0;
            }
            else
            {
                //slidingPT = Game1.GetDungeonTile(position + offset + (velDir * 5));
                if (conveyorDelay <= 0)
                {
                    //Goes to the next point
                    position = Game1.game.getTruncLevel(position + new Vector2(10, 10) + (velDir * 10)) * 20;
                    slidingPT = Game1.GetDungeonTile(position + new Vector2(10, 10)
                        //+ (velDir * 10)
                        );

                    //Checks the direction of said point
                    if (Game1.game.IsSameLayer(slidingPT, "convyE"))
                    {
                        velDir.X = 1;
                        velDir.Y = 0;
                    }
                    else if (Game1.game.IsSameLayer(slidingPT, "convyW"))
                    {
                        velDir.X = -2;
                        velDir.Y = 0;
                    }
                    else if(Game1.game.IsSameLayer(slidingPT, "convyS"))
                    {
                        velDir.X = 0;
                        velDir.Y = 1;
                    }
                    else if(Game1.game.IsSameLayer(slidingPT, "convyN"))
                    {
                        velDir.X = 0;
                        velDir.Y = -2;
                    }
                    if (Game1.game.IsSameLayer(slidingPT, "convyR"))
                    {
                        if (isRed)
                            velDir = new Vector2(2, 0);
                        else
                            velDir = new Vector2(-2, 0);
                    }
                    if (Game1.game.IsSameLayer(slidingPT, "convyB"))
                    {
                        if (isRed)
                            velDir = new Vector2(-2, 0);
                        else
                            velDir = new Vector2(1, 0);
                    }
                    conveyorDelay = 0.15f;
                    if (!Game1.game.IsSameLayer(slidingPT, "convyN") &&
                         !Game1.game.IsSameLayer(slidingPT, "convyW") &&
                         !Game1.game.IsSameLayer(slidingPT, "convyS") &&
                         !Game1.game.IsSameLayer(slidingPT, "convyE") &&
                         !Game1.game.IsSameLayer(slidingPT, "convyR") &&
                         !Game1.game.IsSameLayer(slidingPT, "convyB"))
                    {
                        control = true;
                        return;
                    }
                    Game1.game.PlaySound(3);
                }
                else {
                    conveyorDelay -= Game1.deltaTime;
                }
            }

            if (!noClip)
            {
                ForceCollisionBoxUpdate();
                position.X += velocity.X;
                CollisionDetectionX("solid");

                if (isRed)
                    CollisionDetectionX("redTiles");
                else
                    CollisionDetectionX("blueTiles");
                if (isGrounded)
                    CollisionDetectionX("ditch");

                ForceCollisionBoxUpdate();
                position.Y += velocity.Y;
                CollisionDetectionY("solid");

                if (isRed)
                    CollisionDetectionY("redTiles");
                else
                    CollisionDetectionY("blueTiles");
                if (isGrounded)
                    CollisionDetectionY("ditch");
            }
            else {

                position.X += velocity.X;
                position.Y += velocity.Y;
            }

            if (position.Y < 0)
                position.Y = 0;
            if (position.Y > Game1.g1.curDunMap.mapSize.Y * (20 * (20)) - 40) {
                position.Y = Game1.g1.curDunMap.mapSize.Y * (20 * (20)) - 40;
            }
            System.Console.WriteLine(Game1.g1.curDunMap.mapSize.Y * (20 * (20)) - 40);
            if (position.X < 0)
                position.X = 0;
            if (position.X > Game1.g1.curDunMap.mapSize.X * (20 * (20)) - 40)
                position.X = Game1.g1.curDunMap.mapSize.X * (20 * (20)) - 40;

            if (control)
            {
                slidingPT = Game1.GetDungeonTile(position + new Vector2(10, 10));
                if (Game1.game.IsSameLayer(slidingPT, "convyN") ||
                    Game1.game.IsSameLayer(slidingPT, "convyS") ||
                    Game1.game.IsSameLayer(slidingPT, "convyE") ||
                    Game1.game.IsSameLayer(slidingPT, "convyW") ||
                    Game1.game.IsSameLayer(slidingPT, "convyR") ||
                    Game1.game.IsSameLayer(slidingPT, "convyB"))
                {
                    velocity = Vector2.Zero;
                    if (Game1.game.IsSameLayer(slidingPT, "convyW"))
                        velDir = new Vector2(-1, 0);

                    if (Game1.game.IsSameLayer(slidingPT, "convyE"))
                        velDir = new Vector2(1, 0);

                    if (Game1.game.IsSameLayer(slidingPT, "convyS"))
                        velDir = new Vector2(0, 1);

                    if (Game1.game.IsSameLayer(slidingPT, "convyN"))
                        velDir = new Vector2(0, -1);
                    if (Game1.game.IsSameLayer(slidingPT, "convyR"))
                    {
                        if (isRed)
                            velDir = new Vector2(2, 0);
                        else
                            velDir = new Vector2(-2, 0);
                    }
                    if (Game1.game.IsSameLayer(slidingPT, "convyB"))
                    {
                        if (isRed)
                            velDir = new Vector2(-2, 0);
                        else
                            velDir = new Vector2(1, 0);
                    }

                    //Snaps to the touched tile
                    position = Game1.game.getTruncLevel(position + offset) * 20;
                    control = false;
                }
            }
            animHandler.Update(gametime);

            if (animHandler.GetCurrentFrame != null)
                renderer.SetSprite("player", animHandler.GetCurrentFrame, animHandler.currentAnimation.spriteFx);

            base.Update(gametime);
            renderer.position = position + new Vector2(0, zPos);
            if(!Game1.game.IsSameLayer(slidingPT, "convyN") &&
               !Game1.game.IsSameLayer(slidingPT, "convyW") &&
               !Game1.game.IsSameLayer(slidingPT, "convyS") &&
               !Game1.game.IsSameLayer(slidingPT, "convyE") &&
               !Game1.game.IsSameLayer(slidingPT, "convyR") &&
               !Game1.game.IsSameLayer(slidingPT, "convyB") &&
               !Game1.game.IsSameLayer(slidingPT, "ditch"))
                lastFallPos = position;
        }
    }

    public class o_npc : s_object
    {
        public delegate void ai_function();
        ai_function AI_function;
        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
            if (AI_function != null) {
                AI_function.Invoke();
            }

        }
    }
    public class o_item : s_object
    {
        public float Zpos;
        public float posChangeZ;
        s_animhandler animHandler;
        public override void Start()
        {
            base.Start();
            collisionBox = new Rectangle(5,5,10,10);
            animHandler = new s_animhandler();
            {
                s_anim an = new s_anim("coin");
                an.AddAnimation(0, 0.1f);
                an.AddAnimation(1, 0.1f);
                an.AddAnimation(2, 0.1f);
                an.AddAnimation(1, 0.1f);
                animHandler.AddAnimation(an);
            };
        }
        public enum ITEM_TYPE { 
            COIN
        }
        ITEM_TYPE typeItem;
        public override void Update(GameTime gametime)
        {
            switch (typeItem) {
                case ITEM_TYPE.COIN:
                    animHandler.SetAnimation("coin", true);
                    break;
            }
            base.Update(gametime);
            if (collisionBox.Intersects(Game1.g1.pl.collisionBox))
            {
                Game1.game.PlaySound(6);
                Game1.g1.points++;
                Game1.RemoveObject(this);
            }
            posChangeZ += Game1.deltaTime;
            Zpos = (float)Math.Sin(posChangeZ);

            animHandler.Update(gametime);
            if (animHandler.GetCurrentFrame != null)
                renderer.SetSprite("coin", animHandler.GetCurrentFrame);
            renderer.position = position + new Vector2(0,Zpos);
        }
    }

    public class o_button : s_object
    {
        public override void Update(GameTime gametime)
        {
            renderer.rect.Location = position.ToPoint();
            base.Update(gametime);
        }
    }

    public class o_door { 
    
    }

    public class Game1 : e_solarHeart
    {
        public enum GAME_ENGINE_MODE {
            INTRO,
            MAIN_MENU,
            GAME,
            GAME_OVER,
        };
        GAME_ENGINE_MODE gameMode;

        float preIntroTimer = 0.5f;
        float introTimer = 0.85f;

        int step = 0; int tstep = 0;
        public List<Tuple<string, s_map>> dungeonMaps;
        s_dungeonLayout dunLayout;

        public int points;
        public int highscore;

        Random RNG = new Random(2);
        public o_player pl;

        Texture2D logo;
        Texture2D tileset;
        Texture2D characterSprites;
        Texture2D title;
        Texture2D floor;
        Texture2D coins;

        Point playrPosInMap;

        public static ushort[,] tilesColl;
        public static Game1 g1;

        public bool isDebug = false;

        public s_dungeonMap curDunMap;
        s_dungeonLayout[] dunLayouts;

        public enum GAME_STATE { 
            MAIN_MENU,
            GAME,
            END
        }

        Vector2 cameraOffset;

        public Game1()
        {
            g1 = this;
            Content.RootDirectory = "Content";
        }

        public static void RemoveObject(s_object ob) {
            objects.Remove(ob);
        }

        protected override void Initialize()
        {
            SetVritualScreen(RESOLUTION.R16_9, SCREEN_SIZE.SMALL, RESOLUTION.R16_9, SCREEN_SIZE.LARGE);
            base.Initialize();
            List<s_dungeonLayout> dungeonLayouts = new List<s_dungeonLayout>();

            dunLayout = new s_dungeonLayout();
            dunLayout.leftBottom = new s_map[2] { 
                FindStaticMap("alpha_L_B"), 
                FindStaticMap("gamma_L_B")
            };

            dunLayout.rightBottom = new s_map[3] { 
                FindStaticMap("alpha_R_B"),
                FindStaticMap("alpha_R_B2"),
                FindStaticMap("beta_R_B")
            };

            dunLayout.rightTop = new s_map[2] { 
                FindStaticMap("alpha_R_T"),
                FindStaticMap("beta_R_T")
            };
            dunLayout.bottomTop = new s_map[3] {
                FindStaticMap("alpha_T_B"), 
                FindStaticMap("alpha_T_B2"),
                FindStaticMap("gamma_T_B")
            };
            dunLayout.leftTop = new s_map[2] { 
                FindStaticMap("alpha_L_T"),
                FindStaticMap("alpha_L_T2")
            };
            dunLayout.leftRight = new s_map[4] {
                FindStaticMap("alpha_L_R"),
                FindStaticMap("alpha_L_R2"), 
                FindStaticMap("alpha_L_R3"),
                FindStaticMap("gamma_L_R")
            };
            dunLayout.layoutMaxSize = new Point(6, 6);
            dunLayout.layoutMinSize = new Point(3, 4);
            {
                level = new s_map();
                level.mapSizeX = 10;
                level.mapSizeY = 10;

                level.tileSizeX = 20;
                level.tileSizeY = 20;

                level.tiles = new ushort[level.mapSizeX * level.mapSizeY];
            }

            layers.Add("solid", new List<ushort>() { 1,2});
            layers.Add("convyN", new List<ushort>() { 14 });
            layers.Add("convyS", new List<ushort>() { 12 });
            layers.Add("convyW", new List<ushort>() { 13 });
            layers.Add("convyE", new List<ushort>() { 11 });

            layers.Add("convyR", new List<ushort>() { 8 });
            layers.Add("convyB", new List<ushort>() { 9 });

            layers.Add("door", new List<ushort>() { 3 });
            layers.Add("ditch", new List<ushort>() { 10 });
            layers.Add("redTiles", new List<ushort>() { 4 });
            layers.Add("blueTiles", new List<ushort>() { 5 });

            //layers.Add("slope>", new List<ushort>() { 2 });
            StartGame();
        }

        public void StartGame() {

            objects.Clear();
            GenerateDungeon();

        }
        Point initPosDebug;

        protected override void LoadContent()
        {
            base.LoadContent();

            blank = Content.Load<Texture2D>("blank");
            tileset = Content.Load<Texture2D>("tiles");
            characterSprites = Content.Load<Texture2D>("player");
            title = Content.Load<Texture2D>("title");
            coins = Content.Load<Texture2D>("coin");
            logo = Content.Load<Texture2D>("PBRS_LOGO_2020");
            floor = Content.Load<Texture2D>("floor");

            sounds = new List<SoundEffect>();
            sounds.Add(Content.Load<SoundEffect>("sound/error"));
            sounds.Add(Content.Load<SoundEffect>("sound/land"));
            sounds.Add(Content.Load<SoundEffect>("sound/PrownieOpeningJingle"));
            sounds.Add(Content.Load<SoundEffect>("sound/conveyor"));
            sounds.Add(Content.Load<SoundEffect>("sound/step"));
            sounds.Add(Content.Load<SoundEffect>("sound/colour_change"));
            sounds.Add(Content.Load<SoundEffect>("sound/money_get"));

            CreateSpriteSheets();

            AddTexture(new Point(20, 20), "blank", s_spriterend.SPRITE_SLICE_MODE.DIMENSION, new Point(6, 1));
            AddTexture(new Point(20, 20), "player", s_spriterend.SPRITE_SLICE_MODE.DIMENSION, new Point(3, 2));
            AddTexture(new Point(20, 20), "coin", s_spriterend.SPRITE_SLICE_MODE.DIMENSION, new Point(2, 1));
            AddTexture(new Point(20, 20), "floor", s_spriterend.SPRITE_SLICE_MODE.DIMENSION, new Point(1, 1));
            AddFont();

            List<string> mapStr = new List<string>();
            dungeonMaps = new List<Tuple<string, s_map>>();
            mapStr.Add("test_L_R");
            mapStr.Add("test_L_T");
            mapStr.Add("test_T_B");
            mapStr.Add("test_R_T");
            mapStr.Add("test_R_B");
            mapStr.Add("test_L_B");

            mapStr.Add("alpha_L_R");
            mapStr.Add("alpha_L_R2");
            mapStr.Add("alpha_L_R3");

            mapStr.Add("alpha_L_T");
            mapStr.Add("alpha_L_T2");

            mapStr.Add("alpha_R_B");
            mapStr.Add("alpha_R_B2");

            mapStr.Add("alpha_T_B");
            mapStr.Add("alpha_T_B2");

            mapStr.Add("alpha_R_T");
            mapStr.Add("alpha_L_B");

            mapStr.Add("beta_L_B");
            mapStr.Add("beta_R_T");
            mapStr.Add("beta_R_B");

            mapStr.Add("gamma_L_R");
            mapStr.Add("gamma_L_B");
            mapStr.Add("gamma_T_B");

            mapStr.Add("top_door");
            mapStr.Add("bottom_door");
            mapStr.Add("right_door");
            mapStr.Add("left_door");

            LoadMaps(mapStr, "Maps"); 
            AddMapToDictionary(mapStr);
        }
        public void BackToMenu()
        {
            if (points > highscore)
                highscore = points;
            points = 0;
            gameMode = GAME_ENGINE_MODE.MAIN_MENU;
            objects.Clear();
        }

        public void NextMap() {
            currentLevel++;
            objects.Clear();
            pl.position = new Vector2(100,100);
            GenerateDungeon();
        }
        public void ResetMap()
        {
            objects.Clear();
            pl = null;
            GenerateDungeon();
        }

        public void AddMapToDictionary(List<string> strList) {
            for (int i = 0; i < strList.Count; i++) {
                dungeonMaps.Add(new Tuple<string, s_map>(strList[i], StaticLevels[i]));
            }
        }

        s_map FindStaticMap(string nam) {
            for (int i = 0; i < dungeonMaps.Count; i++) {
                Tuple<string, s_map> mp = dungeonMaps[i];
                if (mp.Item1 == nam)
                    return mp.Item2;
            }
            return null;
        }

        public void GenerateDungeon()
        {
            pl = new o_player();
            pl.position = new Vector2(0, 0);
            pl.renderer = new s_spriterend();

            pl.Start();
            objects.Add(pl);

            pl.position = new Vector2(100, 100);

            Point size = new Point(
                RNG.Next(dunLayout.layoutMinSize.X, dunLayout.layoutMaxSize.X), 
                RNG.Next(dunLayout.layoutMinSize.Y, dunLayout.layoutMaxSize.Y));
            s_map[,] mpLis = new s_map[size.X, size.Y];
            tilesColl = new ushort[size.X * 20, size.Y * 20];
            s_map mp1; {
                mp1 = new s_map();
                mp1.mapSizeX = 20;
                mp1.mapSizeY = 20;

                mp1.tileSizeX = 20;
                mp1.tileSizeY = 20;

                mp1.tiles = new ushort[mp1.mapSizeX * mp1.mapSizeY];

                mp1.tiles[0] = 1;
                mp1.tiles[1] = 1;
                mp1.tiles[2] = 1;
                mp1.tiles[3] = 1;
                mp1.tiles[4] = 1;
                mp1.tiles[5] = 1;
                mp1.tiles[6] = 1;
                mp1.tiles[7] = 1;
                mp1.tiles[8] = 1;
                mp1.tiles[9] = 1;
                mp1.tiles[10] = 1;
                mp1.tiles[11] = 1;
                mp1.tiles[12] = 1;
                mp1.tiles[13] = 1;
                mp1.tiles[14] = 1;
                mp1.tiles[15] = 1;
                mp1.tiles[16] = 1;
                mp1.tiles[17] = 1;
                mp1.tiles[18] = 1;
                mp1.tiles[19] = 1;
                mp1.tiles[20] = 1;

                mp1.tiles[40] = 1;
                mp1.tiles[60] = 1;
                mp1.tiles[80] = 1;
                mp1.tiles[100] = 1;
                mp1.tiles[120] = 1;
                mp1.tiles[140] = 1;
                mp1.tiles[160] = 1;
                mp1.tiles[180] = 1;
                mp1.tiles[200] = 1;
                mp1.tiles[220] = 1;
                mp1.tiles[240] = 1;
                mp1.tiles[260] = 1;
                mp1.tiles[280] = 1;
                mp1.tiles[300] = 1;
                mp1.tiles[320] = 1;
                mp1.tiles[340] = 1;
                mp1.tiles[360] = 1;
                mp1.tiles[380] = 1;

                mp1.tiles[39] = 1;
                mp1.tiles[59] = 1;
                mp1.tiles[79] = 1;
                mp1.tiles[99] = 1;
                mp1.tiles[119] = 1;
                mp1.tiles[139] = 1;
                mp1.tiles[159] = 1;
                mp1.tiles[179] = 1;
                mp1.tiles[199] = 1;
                mp1.tiles[219] = 1;
                mp1.tiles[239] = 1;
                mp1.tiles[259] = 1;
                mp1.tiles[279] = 1;
                mp1.tiles[299] = 1;
                mp1.tiles[319] = 1;
                mp1.tiles[339] = 1;
                mp1.tiles[359] = 1;
                mp1.tiles[379] = 1;
                mp1.tiles[399] = 1;

                mp1.tiles[381] = 1;
                mp1.tiles[382] = 1;
                mp1.tiles[383] = 1;
                mp1.tiles[384] = 1;
                mp1.tiles[385] = 1;
                mp1.tiles[386] = 1;
                mp1.tiles[387] = 1;
                mp1.tiles[388] = 1;
                mp1.tiles[389] = 1;
                mp1.tiles[390] = 1;
                mp1.tiles[391] = 1;
                mp1.tiles[392] = 1;
                mp1.tiles[393] = 1;
                mp1.tiles[394] = 1;
                mp1.tiles[395] = 1;
                mp1.tiles[396] = 1;
                mp1.tiles[397] = 1;
                mp1.tiles[398] = 1;
                mp1.tiles[399] = 1;
            }
            s_map mp2; {
                mp2 = new s_map();
                mp2.mapSizeX = 20;
                mp2.mapSizeY = 20;

                mp2.tileSizeX = 20;
                mp2.tileSizeY = 20;

                mp2.tiles = new ushort[mp1.mapSizeX * mp1.mapSizeY];

                mp2.tiles[0] = 4;
                mp2.tiles[1] = 4;
                mp2.tiles[2] = 4;
                mp2.tiles[3] = 4;
                mp2.tiles[4] = 4;
                mp2.tiles[5] = 4;
                mp2.tiles[6] = 4;
                mp2.tiles[7] = 4;
                mp2.tiles[8] = 4;
                mp2.tiles[9] = 4;
                mp2.tiles[10] = 4;
                mp2.tiles[11] = 4;
                mp2.tiles[12] = 4;
                mp2.tiles[13] = 4;
                mp2.tiles[14] = 4;
                mp2.tiles[15] = 4;
                mp2.tiles[16] = 4;
                mp2.tiles[17] = 4;
                mp2.tiles[18] = 4;
                mp2.tiles[19] = 4;
                mp2.tiles[20] = 4;

                mp2.tiles[40] = 4;
                mp2.tiles[60] = 4;
                mp2.tiles[80] = 4;
                mp2.tiles[100] = 4;
                mp2.tiles[120] = 4;
                mp2.tiles[140] = 4;
                mp2.tiles[160] = 4;
                mp2.tiles[180] = 4;
                mp2.tiles[200] = 4;
                mp2.tiles[220] = 4;
                mp2.tiles[240] = 4;
                mp2.tiles[260] = 4;
                mp2.tiles[280] = 4;
                mp2.tiles[300] = 4;
                mp2.tiles[320] = 4;
                mp2.tiles[340] = 4;
                mp2.tiles[360] = 4;
                mp2.tiles[380] = 4;

                mp2.tiles[39] = 4;
                mp2.tiles[59] = 4;
                mp2.tiles[79] = 4;
                mp2.tiles[99] = 4;
                mp2.tiles[119] =4;
                mp2.tiles[139] = 4;
                mp2.tiles[159] = 4;
                mp2.tiles[179] = 4;
                mp2.tiles[199] = 4;
                mp2.tiles[219] = 4;
                mp2.tiles[239] = 4;
                mp2.tiles[259] = 4;
                mp2.tiles[279] = 4;
                mp2.tiles[299] = 4;
                mp2.tiles[319] = 4;
                mp2.tiles[339] = 4;
                mp2.tiles[359] = 4;
                mp2.tiles[379] = 1;
                mp2.tiles[399] = 1;

                mp2.tiles[381] = 4;
                mp2.tiles[382] = 4;
                mp2.tiles[383] = 4;
                mp2.tiles[384] = 4;
                mp2.tiles[385] = 4;
                mp2.tiles[386] = 4;
                mp2.tiles[387] = 4;
                mp2.tiles[388] = 4;
                mp2.tiles[389] = 4;
                mp2.tiles[390] = 4;
                mp2.tiles[391] = 4;
                mp2.tiles[392] = 4;
                mp2.tiles[393] = 4;
                mp2.tiles[394] = 4;
                mp2.tiles[395] = 4;
                mp2.tiles[396] = 4;
                mp2.tiles[397] = 4;
                mp2.tiles[398] = 4;
                mp2.tiles[399] = 4;
            }

            Point initPos = new Point(0, 0);

            int dir = 0;
            bool firstTime = false;

            mpLis[initPos.X, initPos.Y] = mp1;
            s_map mp = dunLayout.leftRight[0];
            s_map filler = new s_map();
            {
                filler.mapSizeX = 20;
                filler.mapSizeY = 20;

                filler.tileSizeX = 20;
                filler.tileSizeY = 20;

                filler.tiles = new ushort[filler.mapSizeX * filler.mapSizeY];
                for (int i = 0; i < filler.tiles.Length; i++) {
                    filler.tiles[i] = 2;
                }
            }

            mpLis[initPos.X, initPos.Y].tiles[179] = 0;
            mpLis[initPos.X, initPos.Y].tiles[199] = 0;
            mpLis[initPos.X, initPos.Y].tiles[219] = 0;
            mpLis[initPos.X, initPos.Y].tiles[239] = 0;
            mpLis[initPos.X, initPos.Y].tiles[259] = 0;
            pl.position = new Vector2(initPos.X + 200, initPos.Y + 200);

            while (initPos.Y < size.Y - 1)
            {
                switch (dir)
                {
                    //Right
                    case 0:
                        initPos.X++;
                        dir = RNG.Next(0,2);
                        if (initPos.X == size.X - 1) {
                            dir = 1;
                            mp = dunLayout.leftBottom[RNG.Next(0, dunLayout.leftBottom.Length)];
                            break;
                        }
                        if(dir == 0)
                            mp = dunLayout.leftRight[RNG.Next(0, dunLayout.leftRight.Length)];
                        if (dir == 1)
                            mp = dunLayout.leftBottom[RNG.Next(0, dunLayout.leftBottom.Length)];
                        break;

                    //Down
                    case 1:
                        initPos.Y++;
                        dir = RNG.Next(0, 3);
                        if (dir == 0 && initPos.X == size.X - 1) {
                            mp = dunLayout.leftTop[RNG.Next(0, dunLayout.leftTop.Length)];
                            dir = 2;
                            break;
                        }
                        if (dir == 2 && initPos.X == 0) {
                            mp = dunLayout.rightTop[RNG.Next(0, dunLayout.rightTop.Length)];
                            dir = 0;
                            break;
                        }
                        if (initPos.Y == size.Y - 2)
                            dir = 1;
                        if (dir == 1)
                            mp = dunLayout.bottomTop[RNG.Next(0, dunLayout.bottomTop.Length)];
                        if (dir == 0)
                            mp = dunLayout.rightTop[RNG.Next(0, dunLayout.rightTop.Length)];
                        if (dir == 2)
                            mp = dunLayout.leftTop[RNG.Next(0, dunLayout.leftTop.Length)];
                        break;

                    //Left
                    case 2:
                        initPos.X--;
                        dir = RNG.Next(1, 3);
                        if (dir == 2 && initPos.X == 0) {
                            dir = 1;
                            mp = dunLayout.rightBottom[RNG.Next(0, dunLayout.rightBottom.Length)];
                            break;
                        }
                        if (dir == 2)
                            mp = dunLayout.leftRight[RNG.Next(0, dunLayout.leftRight.Length)];
                        if (dir == 1)
                            mp = dunLayout.rightBottom[RNG.Next(0, dunLayout.rightBottom.Length)];
                        break;
                }
                /*
                switch (dir)
                {
                    //Right
                    case 0:
                        initPos.X++;
                        initPos.X = MathHelper.Clamp(initPos.X, 0, size.X - 1);
                        dir = RNG.Next(0, 2);
                        if (initPos.X == size.X)
                        {
                            mp = dunLayout.leftBottom[RNG.Next(0, dunLayout.leftBottom.Length)];
                            dir = 1;
                            break;
                        }
                        if (dir == 0)
                            mp = dunLayout.leftRight[RNG.Next(0, dunLayout.leftRight.Length)];
                        if (dir == 1)
                            mp = dunLayout.leftBottom[RNG.Next(0, dunLayout.leftBottom.Length)];
                        break;

                    //Down
                    case 1:
                        initPos.Y++;
                        dir = RNG.Next(0, 3);
                        if (initPos.X == size.X && dir == 0)
                        {
                            dir = 2;
                            mp = dunLayout.leftTop[RNG.Next(0, dunLayout.leftTop.Length)];
                            break;
                        }
                        if (initPos.X == 0 && dir == 2)
                        {
                            dir = 0;
                            mp = dunLayout.rightTop[RNG.Next(0, dunLayout.rightTop.Length)];
                            break;
                        }
                        if (initPos.Y == size.Y - 2)
                            dir = 1;
                        if (dir == 2)
                            mp = dunLayout.leftTop[RNG.Next(0, dunLayout.leftTop.Length)];
                        if (dir == 0)
                            mp = dunLayout.rightTop[RNG.Next(0, dunLayout.rightTop.Length)];
                        if (dir == 1)
                            mp = dunLayout.bottomTop[RNG.Next(0, dunLayout.bottomTop.Length)];
                        break;

                    //Left
                    case 2:
                        initPos.X--;
                        initPos.X = MathHelper.Clamp(initPos.X, 0, size.X - 1);
                        dir = RNG.Next(0, 2);
                        if (initPos.X <= 0)
                        {
                            mp = dunLayout.rightBottom[RNG.Next(0, dunLayout.rightBottom.Length)];
                            dir = 1;
                            break;
                        }
                        /*
                        if (dir == 1)
                            mp = dunLayout.rightBottom[RNG.Next(0, dunLayout.rightBottom.Length)];
                        if (dir == 0)
                            mp = dunLayout.leftRight[RNG.Next(0, dunLayout.leftRight.Length)];
                        break;
                }
                */
                //initPos.X = MathHelper.Clamp(initPos.X, 0, size.X - 1);
                mpLis[initPos.X, initPos.Y] = mp;
                CreateEntitiesLoad(mp, new Point(initPos.X * 20 * 20, initPos.Y * 20 * 20));
            }

            mpLis[initPos.X, initPos.Y] = FindStaticMap("top_door");
            //pl.position = new Vector2(0, 0);

            for (int x1 = 0; x1 < size.X; x1++) {
                for (int y1 = 0; y1 < size.Y; y1++) {
                    if (mpLis[x1, y1] != null) {
                        for (int x = 0; x < 20; x++) {
                            for (int y = 0; y < 20; y++) {
                                tilesColl[(x1 * 20) + x, (y1 * 20) + y] = mpLis[x1, y1].tiles[x + (y * 20)];
                            }
                        }
                    } 
                    else {
                        mpLis[x1, y1] = filler;
                        for (int x = 0; x < 20; x++) {
                            for (int y = 0; y < 20; y++) {
                                tilesColl[(x1 * 20) + x, (y1 * 20) + y] = 0;
                            }
                        }
                    }
                }
            }
            curDunMap = new s_dungeonMap(mpLis, size);
        }

        public void ChangeTiles() { 
            
        }

        public void CheckCollisionDungeon(s_object ob) {
            Point vecDiv1 = new Point((int)(ob.position.X / 400), (int)(ob.position.Y / 400));
            playrPosInMap = vecDiv1;

        }

        public static ushort GetDungeonTile(Vector2 ob) {
            Point p = new Point((int)(ob.X / 20), (int)(ob.Y / 20));
            if (p.X < 0)
                return 0;
            return tilesColl[p.X, p.Y];
        }

        public override void CreateEntities(o_entity ent)
        {
            base.CreateEntities(ent);
            switch (ent.id) {
                case 0:
                    o_item it = new o_item();
                    it.name = "coin";
                    it.position = new Vector2(ent.position.X, ent.position.Y);
                    it.renderer = new s_spriterend();
                    it.renderer.SetSprite("coin",0);
                    it.Start();
                    objects.Add(it);
                    break;
            
            }
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (gameMode)
            {
                case GAME_ENGINE_MODE.INTRO:
                    if (preIntroTimer > 0)
                    {
                        preIntroTimer -= deltaTime;

                        if (preIntroTimer <= 0)
                            Game1.game.PlaySound(2);
                    }
                    else {
                        introTimer -= deltaTime;
                        if(introTimer <= 0)
                            gameMode = GAME_ENGINE_MODE.MAIN_MENU;
                    }
                    break;

                case GAME_ENGINE_MODE.MAIN_MENU:
                    if (KeyPressed(Keys.Space))
                    {
                        ResetMap();
                        gameMode = GAME_ENGINE_MODE.GAME;
                    }
                    break;

                case GAME_ENGINE_MODE.GAME:

                    // TODO: Add your update logic here
                    CheckCollisionDungeon(pl);
                    camera.camReverse = true;

                    if (isDebug) {
                        if (KeyPressed(Keys.D2))
                            camera.zoom -= 0.01f;
                        if (KeyPressed(Keys.D1))
                            camera.zoom += 0.01f;
                        if (KeyPressed(Keys.D3))
                            camera.angle -= 0.5f;
                        if (KeyPressed(Keys.D4))
                            camera.angle += 0.5f;

                        if (KeyPressed(Keys.D))
                            cameraOffset += new Vector2(2, 0);
                        if (KeyPressed(Keys.A))
                            cameraOffset += new Vector2(-2, 0);
                        if (KeyPressed(Keys.W))
                            cameraOffset += new Vector2(0, -2);
                        if (KeyPressed(Keys.S))
                            cameraOffset += new Vector2(0, 2);
                    }
                    /*
                    if (KeyPressedDown(Keys.R))
                    {
                        objects.Clear();
                        GenerateDungeon();
                    }
                    */

                    camera.zoom = MathHelper.Clamp(camera.zoom, 0.1f, 1f);
                    for (int i =0; i < objects.Count; i++)
                    {
                        s_object o = objects[i];
                        if (o != null)
                            o.Update(gameTime);
                    }
                    camera.Follow(pl.position + new Vector2(30, 30) + cameraOffset);
                    break;
            }

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            switch (gameMode) {

                case GAME_ENGINE_MODE.INTRO:
                    DrawStart(Color.Black);
                    break;
                case GAME_ENGINE_MODE.MAIN_MENU:
                    DrawStart(Color.DeepSkyBlue);
                    break;

                case GAME_ENGINE_MODE.GAME_OVER:
                    DrawStart(Color.Black);
                    break;

                case GAME_ENGINE_MODE.GAME:

                    DrawStart(Color.Black);
                    DrawTiles();
                    foreach (s_object ob in objects)
                    {
                        if (ob.renderer != null)
                            DrawSprite(ob.renderer.texture, new Point(5, 5),
                                new Rectangle(ob.renderer.position.ToPoint(),
                                new Point(20, 20)),
                                new Rectangle(
                                    ob.renderer.rect.Location,
                                    ob.renderer.rect.Size),
                                Color.White,
                                ob.renderer.sprEff, 0,
                                new Vector2(0, 0));
                        else
                        {
                            DrawSprite(blank, new Point(5, 5),
                                new Rectangle(ob.position.ToPoint(),
                                new Point(20, 20)),
                                new Rectangle(0, 0, 20, 20),
                                Color.White,
                                SpriteEffects.None, 0,
                                new Vector2(0, 0));
                        }
                        /*
                        DrawSprite(blank, new Point(5, 5), new Rectangle(pl.slopeStepPos.ToPoint(), new Point(4, 4)),
                            new Rectangle(0, 0, 20, 20),
                            Color.White,
                            SpriteEffects.None, 0,
                            new Vector2(0, 0));

                        DrawSprite(blank, new Point(5, 5), new Rectangle(pl.tilePos.ToPoint(), new Point(1, 1)),
                            new Rectangle(0, 0, 20, 20),
                            Color.Orange,
                            SpriteEffects.None, 0,
                            new Vector2(0, 0));

                        DrawSprite(blank, new Point(5, 5), new Rectangle(pl.position.ToPoint() + pl.offset.ToPoint(), new Point(1, 1)),
                            new Rectangle(0, 0, 20, 20),
                            Color.Orange,
                            SpriteEffects.None, 0,
                            new Vector2(0, 0));
                        */
                    }
                    break;
            }

            base.Draw(gameTime);
            DrawEnd();
        }

        public void DrawMapIcon()
        {
            bool firstOne = true;
            int counter = 0;
            if (curDunMap.area != null)
            {
                int ypos = curDunMap.mapSize.Y;
                for (int x = 0; x < curDunMap.mapSize.X; x++)
                {
                    for (int y = 0; y < curDunMap.mapSize.Y; y++)
                    {
                        //Individual Tiles
                        DrawSprite(blank, new Point(0, 0),
                            new Rectangle(new Point(x * 15, (ypos * -15) + 190), new Point(15, 15)),
                            new Rectangle(0, 0, 15, 15),
                            new Color(160, 20, 20, 40),
                            SpriteEffects.None, 0,
                            new Vector2(0, 0));

                        if (curDunMap.area[x, y] == null)
                        {
                            ypos--;
                            continue;
                        }
                        if (x == playrPosInMap.X && y == playrPosInMap.Y * -1)
                        {
                            DrawSprite(blank, new Point(0, 0),
                                new Rectangle(new Point(x * 15, (ypos * -15) + 190), new Point(15, 15)),
                                new Rectangle(0, 0, 15, 15),
                                Color.Blue,
                                SpriteEffects.None, 0,
                                new Vector2(0, 0));
                        }
                        else
                        {
                            DrawSprite(blank, new Point(0, 0),
                                new Rectangle(new Point(x * 15, (ypos * -15) + 190), new Point(15, 15)),
                                new Rectangle(0, 0, 15, 15),
                                Color.Orange,
                                SpriteEffects.None, 0,
                                new Vector2(0, 0));
                        }
                        ypos--;
                        counter++;
                    }
                    ypos = curDunMap.mapSize.Y;
                }
            }
        }
        public void DrawTiles() {

            int tileSize = 20;
            int realY = 0;
            if (curDunMap.area != null)
                for (int x = 0; x < curDunMap.mapSize.X; x++)
                {
                    for (int y = curDunMap.mapSize.Y - 1; y > -1; y--)
                    {
                        DrawMapDebugTiles(curDunMap.area[x, y], new Point(x, y));
                        if (curDunMap.area[x,y] == null)
                        {
                            continue;
                        }
                        DrawMapTiles(curDunMap.area[x,y], new Point(x, y));
                        realY++;
                    }
                    realY = 0;
                }
        }

        void DrawMapTiles(s_map curDunMap, Point mappos) {

            int multp = 20 * 20;
            int xOffset = 5 + (mappos.X * multp);
            int yOffset = 5 + (mappos.Y * multp); //385

            int posy = 0;
            for (int i = 0; i < curDunMap.tiles.Length; i++)
            {
                ushort til = (ushort)(curDunMap.tiles[i] - 1);
                Point tilePo = GetTexturePT(til);

                if (i % curDunMap.mapSizeX == 0 && i != 0)
                    posy++;

                int posx = i % curDunMap.mapSizeX;

                if (IsSameLayer((ushort)(til + 1), "convyR"))
                {
                    if (pl.isRed)
                    {
                        tilePo = GetTexturePT(7);
                    } else { 
                        tilePo = GetTexturePT(8);
                    }
                }
                else if (IsSameLayer((ushort)(til + 1), "convyB"))
                {
                    if (pl.isRed)
                    {
                        tilePo = GetTexturePT(8);
                    }
                    else
                    {
                        tilePo = GetTexturePT(7);
                    }
                }
                else if (IsSameLayer((ushort)(til + 1), "redTiles"))
                {
                    DrawSprite(floor, new Point(0, 0)
                        , new Rectangle(new Point((posx * curDunMap.tileSizeX) + xOffset, (posy * curDunMap.tileSizeY) + yOffset), new Point(20, 20)),
                        new Rectangle(new Point(0, 0), new Point(20, 20)),
                        Color.White,
                SpriteEffects.None, 0, new Vector2(0, 0));
                    if (pl.isRed)
                    {
                        tilePo = GetTexturePT(4);
                    }
                    else
                    {
                        tilePo = GetTexturePT(5);
                    }
                }
                else if (IsSameLayer((ushort)(til + 1), "blueTiles"))
                {
                    DrawSprite(floor, new Point(0, 0)
                        , new Rectangle(new Point((posx * curDunMap.tileSizeX) + xOffset, (posy * curDunMap.tileSizeY) + yOffset), new Point(20, 20)),
                        new Rectangle(new Point(0, 0), new Point(20, 20)),
                        Color.White,
                SpriteEffects.None, 0, new Vector2(0, 0));
                    if (pl.isRed)
                    {
                        tilePo = GetTexturePT(6);
                    }
                    else
                    {
                        tilePo = GetTexturePT(3);
                    }
                }

                switch (curDunMap.tiles[i]) {
                    default:

                        DrawSprite(tileset, new Point(0, 0)
                            , new Rectangle(new Point((posx * curDunMap.tileSizeX) + xOffset, (posy * curDunMap.tileSizeY) + yOffset), new Point(20, 20)),
                            new Rectangle(new Point(curDunMap.tileSizeX * tilePo.X, curDunMap.tileSizeX * tilePo.Y), new Point(20, 20)),
                            Color.White,
                    SpriteEffects.None, 0, new Vector2(0, 0));
                        break;

                    case 0:
                        DrawSprite(floor, new Point(0, 0)
                            , new Rectangle(new Point((posx * curDunMap.tileSizeX) + xOffset, (posy * curDunMap.tileSizeY) + yOffset), new Point(20, 20)),
                            new Rectangle(new Point(0, 0), new Point(20, 20)),
                            Color.White,
                    SpriteEffects.None, 0, new Vector2(0, 0));
                        break;
                
                }
                if (isDebug)
                    DrawText("" + tilesColl[(mappos.X * 20) + posx, (mappos.Y * 20) + posy], font, new Vector2((posx * curDunMap.tileSizeX) + xOffset, (posy * curDunMap.tileSizeY) + yOffset), spriteBatch);
            }
        }
        void DrawMapDebugTiles(s_map curDunMap, Point mappos)
        {

            int multp = 20 * 20;
            int xOffset = 5 + (mappos.X * multp);
            int yOffset = (mappos.Y * multp);

            int posy = 0;
            if (curDunMap == null)
            {
                for (int i = 0; i < 20 * 20; i++)
                {
                    if (i % 20 == 0 && i != 0)
                        posy++;

                    int posx = i % 20;
                    DrawText("" + tilesColl[(mappos.X * 20) + posx, (mappos.Y * 20) + posy], font, new Vector2((posx * 20) + xOffset, (posy * 20) + yOffset), spriteBatch);
                }
            }
        }

        Point GetTexturePT(ushort til) {

            int tilX = (til % (tileset.Width / level.tileSizeX));
            int tilY = ((til * level.tileSizeX) / tileset.Width);
            return new Point(tilX, tilY);
        }

        public void DrawTileIND()
        {
            int tilHGT = 14;
            int posOFFSETY = 35;
            int posOFFSETX = 0;
            for (int i = 0; i < tilHGT; i++) {

                if (posOFFSETX == 6)
                { 
                    posOFFSETX = 0;
                    posOFFSETY+= 20;
                }

                ushort til = (ushort)i;
                Point tilePo = GetTexturePT(til);
                DrawSprite(tileset, new Point(0, 0)
                    , new Rectangle(new Point((posOFFSETX * level.tileSizeX), level.tileSizeY + posOFFSETY), new Point(20, 20)),
                    new Rectangle(new Point(level.tileSizeX * tilePo.X, level.tileSizeX * tilePo.Y), new Point(20, 20)),
                    Color.White,
                    SpriteEffects.None, 0, new Vector2(0, 0));
                DrawText("" + (til + 1), font, 
                    //new Vector2((posOFFSETX * level.tileSizeX) - 10, 55)
                   new Point((posOFFSETX * level.tileSizeX), level.tileSizeY + posOFFSETY).ToVector2()
                    , spriteBatch);
                posOFFSETX++;
            }
        }

        public override void DrawTextRoutineCode()
        {
            switch (gameMode)
            {
                case GAME_ENGINE_MODE.INTRO:

                    if (preIntroTimer < 0)
                        spriteBatch.Draw(logo, centreOfScreen/2, Color.White);
                    break;
                case GAME_ENGINE_MODE.MAIN_MENU:
                    spriteBatch.Draw(title, new Vector2(45, 55), Color.White);
                    DrawText("Press space to start game", font, new Vector2(10, 15), spriteBatch);
                    DrawText("High score: " + highscore, font, new Vector2(10, 40), spriteBatch);
                    DrawText("Pixel Brownie Software 2020", font, new Vector2(10, 190), spriteBatch);

                    break;

                case GAME_ENGINE_MODE.GAME:
                    DrawText("Coins: " + points, font, new Vector2(0, 15), spriteBatch);
                    if(pl.isRed)
                        DrawText("Red", font, new Vector2(100, 15), spriteBatch);
                    else
                        DrawText("Blue", font, new Vector2(100, 15), spriteBatch);

                    if (isDebug)
                    {
                        DrawText("MapPos:" + playrPosInMap + "\n Player position: " + pl.position + " " + pl.zPos + "\n" + "Tilepos: " + pl.slopeTil + "\n" + "1+2 zoom in/ out" + "\n" + "r to reset", font, new Vector2(0, 15), spriteBatch);
                        DrawTileIND();
                        DrawMapIcon();
                    }
                    break;
            }
            base.DrawTextRoutineCode();
        }
    }

}
