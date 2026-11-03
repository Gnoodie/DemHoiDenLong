using UnityEngine;

namespace DemHoiDenLong.VFX
{
    public static class SkillVFXFactory
    {
        private static Material defaultParticleMaterial;

        private static void EnsureMaterial()
        {
            if (defaultParticleMaterial == null)
            {
                // Create a basic additive material for particles
                Shader shader = Shader.Find("Particles/Standard Unlit");
                if (shader == null) shader = Shader.Find("Mobile/Particles/Additive");
                defaultParticleMaterial = new Material(shader);
            }
        }

        // 1. Kim Lân - Dash Hào Quang
        public static ParticleSystem CreateKimLanAura(Transform parent)
        {
            EnsureMaterial();
            GameObject go = new GameObject("KimLanAura_VFX");
            go.transform.SetParent(parent, false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop();
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.material = defaultParticleMaterial;

            // Main Module
            var main = ps.main;
            main.duration = 1.5f;
            main.startLifetime = 0.5f;
            main.startSpeed = 0f;
            main.startSize = 0.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;

            // Emission
            var em = ps.emission;
            em.rateOverTime = 0f;
            em.rateOverDistance = 15f; // Toned down from 40

            // Shape
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.4f;

            // Color Over Lifetime
            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new ColorUtility().ParseColor("#FFD700"), 0.0f), 
                    new GradientColorKey(Color.white, 0.5f), 
                    new GradientColorKey(Color.white, 1.0f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f), 
                    new GradientAlphaKey(0.8f, 0.5f), 
                    new GradientAlphaKey(0.0f, 1.0f) 
                }
            );
            col.color = grad;

            // Size Over Lifetime
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
            sol.size = new ParticleSystem.MinMaxCurve(1f, curve);

            return ps;
        }

        public static ParticleSystem CreateKimLanBurst(Transform parent)
        {
            EnsureMaterial();
            GameObject go = new GameObject("KimLanBurst_VFX");
            go.transform.SetParent(parent, false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop();
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.material = defaultParticleMaterial;

            var main = ps.main;
            main.duration = 0.5f;
            main.startLifetime = 0.4f;
            main.startSpeed = 5f;
            main.startSize = 0.3f;
            main.loop = false;

            var em = ps.emission;
            em.rateOverTime = 0f;
            em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10) }); // Toned down from 30

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;
            shape.arcMode = ParticleSystemShapeMultiModeValue.BurstSpread;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new ColorUtility().ParseColor("#FFD700"), 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            col.color = grad;

            return ps;
        }

        // 2. Hỏa Lân - Quét Lửa (Cone)
        public static ParticleSystem CreateHoaLanCone(Transform parent)
        {
            EnsureMaterial();
            GameObject go = new GameObject("HoaLanCone_VFX");
            go.transform.SetParent(parent, false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop();
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.material = defaultParticleMaterial;

            var main = ps.main;
            main.duration = 3.0f; // Increased to 3 seconds
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.0f); // Tăng thời gian sống để bay xa hơn (đến ~8 units)
            main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 10f); // Tăng tốc độ bay 
            main.startSize = new ParticleSystem.MinMaxCurve(1.0f, 2.0f); // Lửa to hơn
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.gravityModifier = 0f; // Bỏ gravity để lửa không bị rơi xuống màn hình
            main.playOnAwake = false;

            var em = ps.emission;
            em.rateOverTime = 100f; // Dày lửa hơn
            em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 20f; // Mở rộng góc phun cho khớp với chiều rộng 4f
            shape.radius = 0.5f; // Miệng phun rộng hơn

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new ColorUtility().ParseColor("#C0392B"), 0.0f), 
                    new GradientColorKey(new ColorUtility().ParseColor("#FF7A00"), 0.4f),
                    new GradientColorKey(new ColorUtility().ParseColor("#FFD700"), 0.8f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0.0f, 0.0f), 
                    new GradientAlphaKey(1.0f, 0.2f), 
                    new GradientAlphaKey(0.0f, 1.0f) 
                }
            );
            col.color = grad;

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(0.3f, 1f), new Keyframe(1f, 0f));
            sol.size = new ParticleSystem.MinMaxCurve(1f, curve);

            return ps;
        }

        // 3. Thủy Lân - Sóng Thần
        public static ParticleSystem CreateThuyLanWave(Transform parent)
        {
            EnsureMaterial();
            GameObject go = new GameObject("ThuyLanWave_VFX");
            go.transform.SetParent(parent, false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop();
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.material = defaultParticleMaterial;

            var main = ps.main;
            main.duration = 1.0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;

            var em = ps.emission;
            em.rateOverTime = 200f; // High rate
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 10f;
            shape.arcMode = ParticleSystemShapeMultiModeValue.Random;
            // Arc logic will be controlled by rotating the emitter in PlayerSkillController or setting arc here
            shape.arc = 180f; 

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new ColorUtility().ParseColor("#1B4F72"), 0.0f), 
                    new GradientColorKey(new ColorUtility().ParseColor("#5DADE2"), 0.5f),
                    new GradientColorKey(Color.white, 1.0f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f), 
                    new GradientAlphaKey(0.8f, 0.7f), 
                    new GradientAlphaKey(0.0f, 1.0f) 
                }
            );
            col.color = grad;

            var vol = ps.velocityOverLifetime;
            vol.enabled = true;
            vol.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            vol.y = new ParticleSystem.MinMaxCurve(0f, 5f); // Push forward/up
            vol.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            return ps;
        }

        // 4. Lân Vàng - Mưa Pháo Hoa (Explosion)
        public static ParticleSystem CreateFireworkExplosion(Transform parent)
        {
            EnsureMaterial();
            GameObject go = new GameObject("FireworkExplosion_VFX");
            go.transform.SetParent(parent, false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop();
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.material = defaultParticleMaterial;

            var main = ps.main;
            main.duration = 1.0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
            main.loop = false;
            main.playOnAwake = false;

            var em = ps.emission;
            em.rateOverTime = 0f;
            em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 40) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new ColorUtility().ParseColor("#E91E63"), 0.0f), 
                    new GradientColorKey(new ColorUtility().ParseColor("#FFD700"), 0.5f),
                    new GradientColorKey(Color.white, 1.0f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f), 
                    new GradientAlphaKey(1.0f, 0.6f), 
                    new GradientAlphaKey(0.0f, 1.0f) 
                }
            );
            col.color = grad;
            
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
            sol.size = new ParticleSystem.MinMaxCurve(1f, curve);

            return ps;
        }
    }

    // Extension helper for hex colors
    public class ColorUtility
    {
        public Color ParseColor(string hex)
        {
            Color color;
            if (UnityEngine.ColorUtility.TryParseHtmlString(hex, out color))
            {
                return color;
            }
            return Color.white;
        }
    }
}
