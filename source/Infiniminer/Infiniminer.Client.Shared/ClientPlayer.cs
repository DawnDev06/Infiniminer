/* ----------------------------------------------------------------------------
MIT License

Copyright (c) 2009 Zach Barth
Copyright (c) 2023 Christopher Whitley

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
---------------------------------------------------------------------------- */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Infiniminer
{
    public sealed class ClientPlayer : Player
    {
        // Things that affect animation.
        private Game gameInstance;
        public SpriteModel SpriteModel;

        public override PlayerTeam Team
        {
            get { return base.Team; }
            set
            {
                if (value != base.Team)
                    UpdateSpriteTexture();

                base.Team = value;
            }
        }

        public override PlayerTools Tool
        {
            get { return base.Tool; }
            set
            {
                if (value != base.Tool)
                    UpdateSpriteTexture();

                base.Tool = value;
            }
        }

        public override bool UsingTool
        {
            get { return base.UsingTool; }
            set
            {
                if (value != base.UsingTool)
                {
                    if (value == true)
                        SpriteModel.StartActiveAnimation("3,0.15");
                }

                base.UsingTool = value;
            }
        }

        public override bool IdleAnimation
        {
            get { return base.IdleAnimation; }
            set 
            {
                if (value != base.IdleAnimation)
                {
                    if (value == true)
                        SpriteModel.SetPassiveAnimation("1,0.2");
                    else
                        SpriteModel.SetPassiveAnimation("0,0.2;1,0.2;2,0.2;1,0.2");
                }

                base.IdleAnimation = value;
            }
        }

        public ClientPlayer(Game gameInstance) : base()
        {
            System.Diagnostics.Debug.Assert(gameInstance != null);
            this.gameInstance = gameInstance;

            Texture2D tex = gameInstance.Content.Load<Texture2D>(GenerateTextureName());
            this.SpriteModel = new SpriteModel(gameInstance, 4, tex);
            this.IdleAnimation = true;
        }

        private void UpdateSpriteTexture()
        {
            string contentPath = GenerateTextureName();
            Texture2D texture = gameInstance.Content.Load<Texture2D>(contentPath);
            SpriteModel.SetSpriteTexture(texture);
        }

        private string GenerateTextureName()
        {
            string name = "sprites/tex_sprite_";

            if (Team == PlayerTeam.Red)
            {
                name += "red_";
            }
            else
            {
                name += "blue_";
            }

            switch (Tool)
            {
                case PlayerTools.ConstructionGun:
                case PlayerTools.DeconstructionGun:
                    name += "construction";
                    break;
                case PlayerTools.Detonator:
                    name += "detonator";
                    break;
                case PlayerTools.Pickaxe:
                    name += "pickaxe";
                    break;
                case PlayerTools.ProspectingRadar:
                    name += "radar";
                    break;
                default:
                    name += "pickaxe";
                    break;
            }

            return name;
        }

    }
}
