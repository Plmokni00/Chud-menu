using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Chud.Backend;

[HarmonyPatch(typeof(Slingshot), "GetLaunchVelocity")]
internal class GetLaunchPatch
{
	public static bool enabled;

	public static void Postfix(Slingshot __instance, ref Vector3 __result)
	{
		if (!enabled) return;
		if ((Object)(object)GorillaTagger.Instance == (Object)null || (Object)(object)GorillaTagger.Instance.headCollider == (Object)null)
		{
			return;
		}

		VRRig targetRig = null;
		float bestScore = float.MaxValue;
		Transform head = GorillaTagger.Instance.headCollider.transform;

		foreach (VRRig rig in VRRigCache.ActiveRigs)
		{
			if (rig.isLocal || rig.Creator == null) continue;

			Vector3 toRig = (rig.transform.position - head.position).normalized;
			float distance = Vector3.Distance(head.position, rig.transform.position);
			float angle = Vector3.Angle(head.forward, toRig);
			float score = angle + distance * 0.1f;

			if (score < bestScore)
			{
				bestScore = score;
				targetRig = rig;
			}
		}

		if (targetRig == null || (Object)(object)targetRig.headMesh == (Object)null)
		{
			return;
		}
		if ((Object)(object)__instance.center == (Object)null)
		{
			return;
		}

		Vector3 targetPos = targetRig.headMesh.transform.position;
		Vector3 targetVel = targetRig.LatestVelocity();
		targetVel.y /= 3f;

		Vector3 origin = __instance.center.transform.position;
		Vector3 displacement = targetPos - origin;
		Vector3 displacementXZ = new Vector3(displacement.x, 0, displacement.z);

		float g = -Physics.gravity.y;
		float x = displacementXZ.magnitude;
		float roughSpeed = 20f;
		float time = x / roughSpeed;

		Vector3 futurePos = targetPos + targetVel * time;
		displacement = futurePos - origin;
		displacementXZ = new Vector3(displacement.x, 0, displacement.z);
		float y = displacement.y;
		x = displacementXZ.magnitude;

		float minSpeed = Mathf.Sqrt(g * (y + Mathf.Sqrt(x * x + y * y)));
		float launchSpeed = Mathf.Max(minSpeed * 2.5f, 5f);

		__result = CalcVelocity(displacement, launchSpeed);
	}

	private static Vector3 CalcVelocity(Vector3 displacement, float speed)
	{
		Vector3 displacementXZ = new Vector3(displacement.x, 0, displacement.z);
		float x = displacementXZ.magnitude;
		float y = displacement.y;
		float g = -Physics.gravity.y;
		float v2 = speed * speed;

		float underSqrt = v2 * v2 - g * (g * x * x + 2 * y * v2);
		if (underSqrt <= 0f) return displacement.normalized * speed;

		float sqrt = Mathf.Sqrt(underSqrt);
		if (x < 0.001f)
		{
			return Vector3.up * speed;
		}

		float angle = Mathf.Atan((v2 - sqrt) / (g * x));

		Vector3 dirXZ = displacementXZ.normalized;
		return dirXZ * Mathf.Cos(angle) * speed + Vector3.up * Mathf.Sin(angle) * speed;
	}
}
