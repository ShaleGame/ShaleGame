using Godot;
using CrossedDimensions.Characters;

namespace CrossedDimensions.States.Characters;

/// <summary>
/// Provides corner correction helpers for jump and dash movement.
/// </summary>
internal static class CharacterCornerCorrection
{
    /// <summary>
    /// Attempts to apply horizontal corner correction while moving upward into
    /// a ceiling.
    /// </summary>
    /// <param name="character">The character to correct.</param>
    /// <returns>
    /// <c>true</c> if a correction was applied; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryApplyUpward(Character character)
    {
        if (!character.EnableCornerCorrection)
        {
            return false;
        }

        if (character.IsOnFloor())
        {
            return false;
        }

        if (character.VelocityFromExternalForces.Y >= 0)
        {
            return false;
        }

        if (!character.IsOnCeiling())
        {
            return false;
        }

        int maxPixels = Mathf.Max(0, character.CornerCorrectionMaxPixels);
        int step = Mathf.Max(1, character.CornerCorrectionStepPixels);

        Vector2 correctionAxis = Vector2.Right;

        return TryApplyDirectionalCorrection(
            character: character,
            correctionAxis: correctionAxis,
            continuationProbe: Vector2.Up * step,
            inputDirectionBias: character.Controller.MovementInput.Dot(correctionAxis),
            velocityDirectionBias: character.Velocity.Dot(correctionAxis),
            targetDirectionBias: character.Controller.Target.Dot(correctionAxis),
            maxPixels: maxPixels,
            stepPixels: step);
    }

    /// <summary>
    /// Attempts to apply corner correction during a dash/split movement.
    /// </summary>
    /// <param name="character">The character to correct.</param>
    /// <param name="dashDirection">The intended dash direction.</param>
    /// <returns>
    /// <c>true</c> if a correction was applied; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryApplyDash(Character character, Vector2 dashDirection)
    {
        if (!character.EnableCornerCorrection)
        {
            return false;
        }

        if (dashDirection.IsZeroApprox())
        {
            return false;
        }

        if (character.GetSlideCollisionCount() == 0)
        {
            return false;
        }

        int maxPixels = Mathf.Max(0, character.CornerCorrectionMaxPixels);
        int step = Mathf.Max(1, character.CornerCorrectionStepPixels);
        if (maxPixels == 0)
        {
            return false;
        }

        Vector2 dashUnit = dashDirection.Normalized();
        if (!TryGetBestCollisionNormalAgainst(character, dashUnit, out Vector2 dashHitNormal))
        {
            return false;
        }

        // Correction needs to move perpendicular to the blocking surface normal,
        // so we derive a tangent axis from that normal.
        Vector2 correctionAxis = dashHitNormal.Orthogonal();
        if (correctionAxis.IsZeroApprox())
        {
            return false;
        }

        Vector2 continuationProbe = dashUnit * step;

        float inputBias = character.Controller.MovementInput.Dot(correctionAxis);
        float velocityBias = character.Velocity.Dot(correctionAxis);
        float targetBias = character.Controller.Target.Dot(correctionAxis);

        return TryApplyDirectionalCorrection(
            character: character,
            correctionAxis: correctionAxis,
            continuationProbe: continuationProbe,
            inputDirectionBias: inputBias,
            velocityDirectionBias: velocityBias,
            targetDirectionBias: targetBias,
            maxPixels: maxPixels,
            stepPixels: step);
    }

    /// <summary>
    /// Tries offsets along a correction axis until one allows continued
    /// movement along the continuation probe.
    /// </summary>
    /// <param name="character">The character to move.</param>
    /// <param name="correctionAxis">Axis along which to apply correction.</param>
    /// <param name="continuationProbe">
    /// Movement that must remain unblocked after correction.
    /// </param>
    /// <param name="inputDirectionBias">
    /// Input-derived direction bias along the correction axis.
    /// </param>
    /// <param name="velocityDirectionBias">
    /// Velocity-derived direction bias along the correction axis.
    /// </param>
    /// <param name="targetDirectionBias">
    /// Aim/target-derived direction bias along the correction axis.
    /// </param>
    /// <param name="maxPixels">Maximum correction distance in pixels.</param>
    /// <param name="stepPixels">Step size for each correction attempt in pixels.</param>
    /// <returns>
    /// <c>true</c> if a valid correction offset is found; otherwise, <c>false</c>.
    /// </returns>
    private static bool TryApplyDirectionalCorrection(
        Character character,
        Vector2 correctionAxis,
        Vector2 continuationProbe,
        float inputDirectionBias,
        float velocityDirectionBias,
        float targetDirectionBias,
        int maxPixels,
        int stepPixels)
    {
        if (maxPixels <= 0)
        {
            return false;
        }

        if (correctionAxis.IsZeroApprox())
        {
            return false;
        }

        Vector2 axis = correctionAxis.Normalized();

        bool TryOffset(int signedOffset)
        {
            Vector2 correction = axis * signedOffset;
            Transform2D shifted = character.GlobalTransform.Translated(correction);
            if (character.TestMove(shifted, Vector2.Zero))
            {
                return false;
            }

            if (!continuationProbe.IsZeroApprox() && character.TestMove(shifted, continuationProbe))
            {
                return false;
            }

            character.GlobalPosition += correction;
            return true;
        }

        int inputDirection = (int)Mathf.Sign(inputDirectionBias);
        if (inputDirection != 0)
        {
            // Bias correction toward explicit player input first for consistency.
            for (int offset = stepPixels; offset <= maxPixels; offset += stepPixels)
            {
                if (TryOffset(inputDirection * offset))
                {
                    return true;
                }
            }

            return false;
        }

        int velocityDirection = (int)Mathf.Sign(velocityDirectionBias);
        if (velocityDirection != 0)
        {
            // If there is no input bias, prefer the existing movement direction.
            for (int offset = stepPixels; offset <= maxPixels; offset += stepPixels)
            {
                if (TryOffset(velocityDirection * offset))
                {
                    return true;
                }
            }

            return false;
        }

        int targetDirection = (int)Mathf.Sign(targetDirectionBias);
        if (targetDirection == 0)
        {
            targetDirection = 1;
        }

        // Last resort: try both signs symmetrically so neutral input/velocity
        // still has a deterministic search order.
        for (int offset = stepPixels; offset <= maxPixels; offset += stepPixels)
        {
            if (TryOffset(targetDirection * offset))
            {
                return true;
            }

            if (TryOffset(-targetDirection * offset))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Selects the collision normal that most strongly opposes the provided
    /// movement direction.
    /// </summary>
    /// <param name="character">The character providing slide collision data.</param>
    /// <param name="movementDirection">Direction to compare against collision normals.</param>
    /// <param name="normal">The best opposing collision normal when found.</param>
    /// <returns>
    /// <c>true</c> if an opposing normal is found; otherwise, <c>false</c>.
    /// </returns>
    private static bool TryGetBestCollisionNormalAgainst(
        Character character,
        Vector2 movementDirection,
        out Vector2 normal)
    {
        normal = Vector2.Zero;

        if (movementDirection.IsZeroApprox())
        {
            return false;
        }

        Vector2 movement = movementDirection.Normalized();
        float bestOpposition = 0.0f;
        int collisionCount = character.GetSlideCollisionCount();

        for (int i = 0; i < collisionCount; i++)
        {
            var collision = character.GetSlideCollision(i);
            if (collision is null)
            {
                continue;
            }

            Vector2 candidateNormal = collision.GetNormal();
            if (candidateNormal.IsZeroApprox())
            {
                continue;
            }

            // opposition = -dot(movement, normal):
            //  1.0 means perfectly head-on collision,
            //  0.0 means perpendicular grazing contact,
            // <0 means the surface normal is not opposing travel.
            float opposition = -movement.Dot(candidateNormal.Normalized());
            if (opposition > bestOpposition)
            {
                bestOpposition = opposition;
                normal = candidateNormal;
            }
        }

        return bestOpposition > 0.001f;
    }
}
