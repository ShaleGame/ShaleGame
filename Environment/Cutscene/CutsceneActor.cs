using Godot;

namespace CrossedDimensions.Environment.Cutscene;

public partial class CutsceneActor : CrossedDimensions.Characters.Character
{
    public override void _Ready()
    {
        IsolateAnimationTree();
        base._Ready();
    }

    private void IsolateAnimationTree()
    {
        var animationTree = GetNodeOrNull<AnimationTree>("AnimationTree");
        if (animationTree is null)
        {
            return;
        }

        if (animationTree.Get("tree_root").As<GodotObject>() is not Resource treeRoot)
        {
            return;
        }

        animationTree.Set("tree_root", treeRoot.Duplicate(true));
    }
}
