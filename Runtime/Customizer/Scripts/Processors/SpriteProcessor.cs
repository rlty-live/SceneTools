using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using JetBrains.Annotations;
using RLTY.SessionInfo;

namespace RLTY.Customisation
{
    [AddComponentMenu("RLTY/Customisable/Processors/Sprite"), HideMonoScript]
    public class SpriteProcessor : Processor
    {
        #region Global Variables
        private SpriteRenderer spriteRenderer;
        private Image image;

        [Title("Parameters")]
        [SerializeField, ShowIf("equiBordersWidth", true)]
        public float equiBordersWidth = 1f;
        [SerializeField, ShowIf("showUtilities")]
        private bool addBorders = true;
        [SerializeField, Range(10, 400)]
        private float minPixelPerUnit = 100;
        [SerializeField, Tooltip("If new Sprite is narrower than placeholder it will be aligned according to selection.")]
        private HorizontalAlignment horizontalAligmbent;
        [SerializeField, Tooltip("If new Sprite is smaller than placeholder it will be aligned according to selection.")]
        private VerticalAlignment verticalAlignment;


        [Title("Placeholder")]
        [InfoBox("Cannot have any children for now.", InfoMessageType.Warning)]
        [SerializeField, ReadOnly, ShowIf("showUtilities")]
        [LabelText("World dimensions")]
        private Vector2 placeholderSpriteWorldDimensions;
        [SerializeField, ReadOnly, ShowIf("showUtilities")]
        [LabelText("world Sprite ratio")]
        private float placeHolderWorldRatio;
        [SerializeField, ReadOnly, ShowIf("showUtilities")]
        [LabelText("Proportions")]
        private TextureProportions placeholderProportions;
        [SerializeField, ShowIf("showUtilities")]
        private bool displayBounds = true;

        private Vector2 placeHolderSpriteDimensions;
        private float placeHolderRatio;
        private float placeholderPPU;

        [Title("New sprite")]
        [SerializeField, ReadOnly, ShowIf("showUtilities")]
        [LabelText("Sprite dimensions")]
        private Vector2 newSpriteDimensions;
        [SerializeField, ReadOnly, ShowIf("showUtilities")]
        [LabelText("Ratio")]
        private float newTextureRatio;
        [SerializeField, ReadOnly, ShowIf("showUtilities")]
        [LabelText("Proportions")]
        private TextureProportions newSpriteProportions;
        [SerializeField, ReadOnly, ShowIf("showUtilities")]
        [LabelText("World dimensions")]
        private Vector2 newSpriteWorldDimensions;
        [SerializeField, ReadOnly, ShowIf("showUtilities")]
        float newRatio;
        [SerializeField, ReadOnly, ShowIf("showUtilities")]
        bool isOffset = false;
        Vector3 PositionOffset;
        float offset;

        #endregion

        public override Component FindComponent(Component existingTarget)
        {
            SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            Image image = GetComponentInChildren<Image>();
            Component target = null;

            if (!spriteRenderer && !image)
            {
                if (debug)
                    Debug.LogWarning("No SpriteRenderer found in children" + commonWarning, this);
            }
            else
            {
                if (spriteRenderer)
                    target = spriteRenderer;

                if (image)
                    target = image;
            }
            return target;
        }

        public override void Customize(Component target, KeyValueBase keyValue)
        {
            Texture t = keyValue.data as Texture;
            if (t == null)
                return;
            if (target.GetType() == typeof(SpriteRenderer))
                SpriteRendererSpriteSwap(t);
            else
            if (target.GetType() == typeof(Image))
                ImageSpriteSwap(t);
        }

        #region Runtime Logic
        //[Button("Measure")]
        public void GetSpriteDimensions()
        {
            if (spriteRenderer.sprite != null)
            {
                placeholderPPU = spriteRenderer.sprite.pixelsPerUnit;
                placeHolderRatio = spriteRenderer.sprite.texture.width / spriteRenderer.sprite.texture.height;
                placeHolderSpriteDimensions = new Vector2(spriteRenderer.sprite.texture.width / placeholderPPU, spriteRenderer.sprite.texture.height / placeholderPPU);
                placeholderSpriteWorldDimensions = placeHolderSpriteDimensions * transform.localScale;
                placeHolderWorldRatio = placeholderSpriteWorldDimensions.x / placeholderSpriteWorldDimensions.y;
            }
            //Local bounds seems to only use the original sprite size, at least in editor (check the Gizmos in editor)
            //spriteWorldDimensions = new Vector2(spriteRenderer.localBounds.size.x, spriteRenderer.localBounds.size.y);

            float spriteRatio = placeholderSpriteWorldDimensions.x / placeholderSpriteWorldDimensions.y;

            switch (spriteRatio)
            {
                case > 1:
                    placeholderProportions = TextureProportions.Landscape;
                    break;
                case < 1:
                    placeholderProportions = TextureProportions.Portrait;
                    break;
                case 1:
                    placeholderProportions = TextureProportions.Square;
                    break;
                default:
                    Debug.Log("Invalid dimensions for " + spriteRenderer + " verify scale and Sprite asset.", this);
                    break;
            }
        }

        public Sprite SetUpSprite(Texture2D tex)
        {
            Rect texturePart = new Rect(0.0f, 0.0f, tex.width, tex.height);

            Vector4 borders;
            if (addBorders)
                borders = new Vector4(equiBordersWidth, equiBordersWidth, equiBordersWidth, equiBordersWidth);
            else
                borders = Vector4.zero;

            Vector2 pivotPosition = new Vector2(0.5f, 0.5f);

            newSpriteDimensions = new Vector2(
                tex.width / spriteRenderer.sprite.pixelsPerUnit,
                tex.height / spriteRenderer.sprite.pixelsPerUnit);
            newTextureRatio = newSpriteDimensions.x / newSpriteDimensions.y;

            switch (newTextureRatio)
            {
                case (> 1):
                    newSpriteProportions = TextureProportions.Landscape;
                    break;
                case (< 1):
                    newSpriteProportions = TextureProportions.Portrait;
                    break;
                case (1):
                    newSpriteProportions = TextureProportions.Square;
                    break;
                default:
                    Debug.Log("Invalid texture dimensions for " + tex + " verify source asset.", this);
                    break;
            }

            return Sprite.Create(tex, texturePart, pivotPosition, minPixelPerUnit, 0, SpriteMeshType.FullRect, borders);
            // Borders could be used to add padding, but it might be better if it's done on the web API
            // Each customisable will have its own set of dimensions to adjust the sprite to
        }

        public void ImageSpriteSwap(Texture tex)
        {
            Image image = GetComponent<Image>();
            image.sprite = SetUpSprite((Texture2D)tex);
        }

        public void SwapSprite(Texture tex)
        {
            GetSpriteDimensions();

            Sprite newSprite = SetUpSprite((Texture2D)tex);
            spriteRenderer.sprite = newSprite;

            float spriteScaleFactor = 1;
            newRatio = 1;

            if (spriteRenderer.drawMode == SpriteDrawMode.Simple)
            {
                switch (placeHolderWorldRatio - newTextureRatio)
                {
                    //New Sprite is narrower than Placeholder
                    case > 0:
                        spriteScaleFactor = placeHolderSpriteDimensions.y / newSpriteDimensions.y;
                        transform.localScale = new Vector3(
                            transform.localScale.y * spriteScaleFactor,
                            transform.localScale.y * spriteScaleFactor,
                            1);
                        break;

                    //New Sprite is wider than Placeholder
                    case < 0:
                        spriteScaleFactor = placeHolderSpriteDimensions.x / newSpriteDimensions.x;
                        transform.localScale = new Vector3(
                            transform.localScale.x * spriteScaleFactor,
                            transform.localScale.x * spriteScaleFactor,
                            1);
                        break;

                    //Both sprites have same ratio
                    case 0:
                        //No need to adujst, just scale
                        spriteScaleFactor = placeholderSpriteWorldDimensions.x / newSpriteDimensions.x;
                        transform.localScale = new Vector3(
                            transform.localScale.x * spriteScaleFactor,
                            transform.localScale.y * spriteScaleFactor,
                            1);
                        break;
                }

                newSpriteWorldDimensions = new Vector2(
                    newSpriteDimensions.x * transform.localScale.x,
                    newSpriteDimensions.y * transform.localScale.y);

                newRatio = newSpriteWorldDimensions.x / newSpriteWorldDimensions.y;

                SetPositionToAnchor();
            }
        }

        public void SetPositionToAnchor()
        {
            switch (horizontalAligmbent)
            {
                case HorizontalAlignment.Center:
                    isOffset = false;
                    break;

                case HorizontalAlignment.Left:
                    offset = (placeholderSpriteWorldDimensions.x - newSpriteWorldDimensions.x) / 2;
                    PositionOffset = new Vector3(-offset, 0, 0);
                    transform.position += PositionOffset;
                    isOffset = true;
                    break;

                case HorizontalAlignment.Right:
                    offset = (placeholderSpriteWorldDimensions.x - newSpriteWorldDimensions.x) / 2;
                    PositionOffset = new Vector3(+offset, 0, 0);
                    transform.position += PositionOffset;
                    isOffset = true;
                    break;
            }

            switch (verticalAlignment)
            {
                case VerticalAlignment.Up:

                    offset = (placeholderSpriteWorldDimensions.y - newSpriteWorldDimensions.y) / 2;
                    PositionOffset = new Vector3(0, offset, 0);
                    transform.position += PositionOffset;
                    isOffset = true;
                    break;

                case VerticalAlignment.Down:
                    offset = (placeholderSpriteWorldDimensions.y - newSpriteWorldDimensions.y) / 2;
                    PositionOffset = new Vector3(0, -offset, 0);
                    transform.position += PositionOffset;
                    isOffset = true;
                    break;
            }
        }

        public static Transform[] GetTopLevelChildren(Transform Parent)
        {
            Transform[] children = new Transform[Parent.childCount];
            for (int ID = 0; ID < Parent.childCount; ID++)
            {
                children[ID] = Parent.GetChild(ID);
            }
            return children;
        }

        public override void CheckSetup()
        {
            base.CheckSetup();

            if (TryGetComponent(out SpriteRenderer _spriteRenderer))
                spriteRenderer = _spriteRenderer;

            if (TryGetComponent(out Image _image))
                image = _image;

            if (!TryGetComponent(out SpriteRenderer spriteRdr2) && !TryGetComponent(out Image _image2))
            {
                correctSetup = false;
                if (debug) Debug.Log("Customisable doesn't have neither a Sprite Renderer nor Image component, please add one or remove this customisable", this);
                return;
            }

            if (!correctSetup)
            {
                correctSetup = false;
                if (debug) Debug.Log("Setup of this customisable Processor is wrong, check debug in Utilies and then check console");
                return;
            }

            correctSetup = true;
        }


        public override void EventHandlerRegister() { }
        public override void EventHandlerUnRegister() { }
        #endregion

        #region EditorOnly Logic
        [Button("Test")]
        public void SpriteRendererSpriteSwap(Texture tex)
        {
            GetSpriteDimensions();
            SwapSprite(tex);
        }

        public void OnValidate()
        {
            CheckSetup();
            if (spriteRenderer)
                GetSpriteDimensions();
        }

        public void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            if (displayBounds && spriteRenderer)
            {
                switch (spriteRenderer.drawMode)
                {
                    case SpriteDrawMode.Simple:
                        Gizmos.DrawWireCube(Vector3.zero, new Vector3(
                            placeholderSpriteWorldDimensions.x / transform.localScale.x,
                            placeholderSpriteWorldDimensions.y / transform.localScale.y,
                            0.1f));
                        break;
                    case SpriteDrawMode.Sliced:
                        Gizmos.DrawWireCube(Vector3.zero, new Vector3(spriteRenderer.size.x, spriteRenderer.size.y, 0.1f));
                        break;
                    case SpriteDrawMode.Tiled:
                        Gizmos.DrawWireCube(Vector3.zero, new Vector3(spriteRenderer.size.x, spriteRenderer.size.y, 0.1f));
                        break;
                }
            }
        }

        public void Reset()
        {
            isOffset = false;
        }
        #endregion
    }
}
