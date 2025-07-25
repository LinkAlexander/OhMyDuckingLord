﻿using OpenTK.Mathematics;

namespace cgimin.engine.object3d;

public class PickingRay
{
    
        private Vector3 origin;
        private Vector3 direction;
        private Vector3 destination;
        //Ray gets defined with 2 Vectors which are the origin and the destination
        public PickingRay(Vector3 origin, Vector3 destination)
        {
            this.origin = origin;
            this.destination = destination;
            direction = (destination - this.origin).Normalized();
        }
    
        public Vector3 Origin
        {
            get { return origin; }
        }
        public Vector3 Destination
        {
            get { return destination; }
        }
        public Vector3 Direction
        {
            get { return direction; }
        }
    
        public Vector3 GetPoint(float distance)
        {
            return origin + direction * distance;
        }
    
        public override string ToString()
        {
            return "Origin: " + origin + ", Destination: " + destination + ", Direction: " + direction;
        }
        //Override the * operator to multiply the ray with a scalar. Needed for collision detection
        public static PickingRay operator *(PickingRay ray, float scalar) {
            return new PickingRay(ray.Origin * scalar, ray.Direction * scalar);
        }
}