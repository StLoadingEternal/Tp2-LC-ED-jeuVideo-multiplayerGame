using Unity.Netcode;
using UnityEngine;

public class SpectatorManager : NetworkBehaviour

{
    //Spectateurs, source audio et clip audio
    [SerializeField] private Animator[] spectatorAnimators;
    [SerializeField] private AudioSource crowdAudioSource;
    [SerializeField] private AudioClip collisionCheerClip;

    //Cooldown
    [SerializeField] private float reactionCooldown = 2f;
    private float lastReactionTime = -999f;

    //Reaction des spectateurs à une collision avec cooldown
    public void TriggerCollisionReaction()
    {
        if (!IsServer) return;

        if (Time.time - lastReactionTime < reactionCooldown) return;
        lastReactionTime = Time.time;

        PlayReactionClientRpc();
    }

    //Jouer l'animation pour tous les clients
    [ClientRpc]
    private void PlayReactionClientRpc()
    {
        foreach (Animator animator in spectatorAnimators)
        {
            if (animator != null)
            {
                animator.SetTrigger("Cheer");
            }
        }

        if (crowdAudioSource != null && collisionCheerClip != null)
        {
            crowdAudioSource.PlayOneShot(collisionCheerClip);
        }
    }

}
