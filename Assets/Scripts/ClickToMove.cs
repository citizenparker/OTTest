using UnityEngine;
using System.Collections;
using System;

public class ClickToMove : MonoBehaviour {
	public LayerMask mask;
	public float locationFuzzRange = 0.1f;
	public float topSpeed = 10f;
	public float timeToTopSpeed = 1.5f;
	public float turnRate = (float)Math.PI / 2f;
	
	private float currentDriveTime = 0f;
	private Nullable<Vector3> destination;
	
	void Update() {
		RefreshDestination();
		MoveTowardsDestination();
	}
	
	void RefreshDestination() {
		if(Input.GetMouseButtonDown(0)) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, 200f, mask.value) ) {
				destination = new Vector3(hit.point.x, this.transform.position.y, hit.point.z);
			}
		}
	}
	
	void MoveTowardsDestination() {
		if(!destination.HasValue || Vector3.Distance(destination.Value, this.transform.position) < locationFuzzRange) {
			currentDriveTime = 0f;
			return;
		}
			
		Turn();
		ApplyGas();
	}
	
	void Turn() {
		Vector3 targetDir = destination.Value - transform.position;
        float step = turnRate * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
		transform.Rotate(this.transform.up, Mathf.Rad2Deg * step);
        transform.rotation = Quaternion.LookRotation(newDir);
	}
	
	void ApplyGas() {
		currentDriveTime = Mathf.Clamp(currentDriveTime + Time.deltaTime, 0, timeToTopSpeed);
		float driveTimeInRadians = Mathf.Lerp(-1.0f, Mathf.PI / 2.0f, currentDriveTime / timeToTopSpeed);
		float currentSpeed = ClampToMaxSpeedForDestination(topSpeed * ((0.5f * Mathf.Sin(driveTimeInRadians)) + 0.5f));
		
		this.transform.position += this.transform.forward * Time.deltaTime * currentSpeed;
	}
	
	float ClampToMaxSpeedForDestination(float targetSpeed) {
		Vector3 localDestination = this.transform.InverseTransformPoint(destination.Value);
		float radius = targetSpeed / turnRate;
		
		if(Mathf.Pow(Mathf.Abs(localDestination.x) - radius, 2f)
		   + Mathf.Pow(localDestination.z, 2f)
		   < Mathf.Pow(radius, 2f)) {
			float angle = Mathf.Deg2Rad * Vector3.Angle(localDestination, new Vector3(localDestination.x, 0f, 0f - localDestination.z));
			float newRadius = Mathf.Abs(localDestination.z) / Mathf.Sin(angle);
			return newRadius * turnRate;
		}
		
		return targetSpeed;
	}
}