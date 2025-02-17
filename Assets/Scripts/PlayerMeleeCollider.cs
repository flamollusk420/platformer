using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMeleeCollider : MonoBehaviour {
    private SoundManager soundManager;
    private PlayerController player;
    private float damageDealt;
    private float dirX;
    public bool canDoDamage;
    private bool canDoDamage2;

    void Start() {
        soundManager = GameObject.FindWithTag("SoundManager").GetComponent<SoundManager>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    public void Attack(float damageDealtPlayer, float playerDirX) {
        damageDealt = damageDealtPlayer;
        dirX = playerDirX;
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if((collision.gameObject.CompareTag("Enemies")) && ((player.facingDirX == 1 && collision.transform.position.x >= player.transform.position.x) || (player.facingDirX == -1 && collision.transform.position.x <= player.transform.position.x))) {
            if(collision.GetComponent<Enemy>() != null && canDoDamage && !player.recoiling) {
                Enemy enemyScript = collision.GetComponent<Enemy>();
                if(enemyScript.canTakeDamage) {
                    player.sp += 2;
                    if(enemyScript.beingKnockedBack) {
                        player.style += 8;
                    }
                }
                if(enemyScript.canTakeDamage) {
                    player.style += 2;
                    player.ResetStyleDeductionTimer();
                    if(player.touchingWall && player.GetComponent<Rigidbody2D>().velocity.y < 0) {
                        player.style += 2;
                    }
                    if(enemyScript.health - damageDealt <= 0) {
                        player.style += 1;
                        if(enemyScript.health == enemyScript.maxHealth) {
                            player.style += 2;
                        }
                    }
                }
                enemyScript.Hit(damageDealt);
                soundManager.PlayClip(soundManager.PlayerMeleeHit, transform, 1f);
                if(enemyScript.canBeKnockedBack) {
                    if(!(enemyScript.getStunnedInsteadOfBeingKnockedBack && enemyScript.knockbackTimer > 0)) {
                        enemyScript.KnockBack(false, false, null, dirX, 0, 0, 1, 1);
                    }
                }
            }
        }
        if(collision.CompareTag("EnemyBullet") && collision.GetComponent<EnemyBullet>() != null && canDoDamage && !collision.GetComponent<EnemyBullet>().fadingOut && !collision.GetComponent<EnemyBullet>().stopped) {
            EnemyBullet collisionBullet = collision.GetComponent<EnemyBullet>();
            if(collisionBullet.canBeDeflected) {
                if(collisionBullet.parryable) {
                    collisionBullet.Deflect();
                    if(player.isDashJumping2) {
                        player.style += 1;
                    }
                }
                if(!collisionBullet.parryable && collisionBullet.canBeDestroyedByPlayer) {
                    soundManager.PlayClip(soundManager.BulletCollision, transform, 1);
                    collisionBullet.canBeDeflected = false;
                    collisionBullet.deflectionTimer = 0.5f;
                    collisionBullet.stopped = true;
                    collisionBullet.fadingOut = true;
                    collisionBullet.fadeOutTimer = collisionBullet.fadeOutTimerSet;
                    FlexibleAnimation explosion = BulletCollisionObjectPool.instance.GetPooledObject().GetComponent<FlexibleAnimation>();
                    explosion.gameObject.SetActive(true);
                    explosion.currentFrame = 0;
                    explosion.transform.position = Vector3.Lerp(transform.position, collision.transform.position, 1);
                    player.style += 1;
                    player.sp += 1;
                }
            }
        }
    }
}
