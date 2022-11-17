using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FluidVolume
{
    int size; //volume size

    float dt; //delta time
    float diff; //diffusion
    float visc; //viscosity

    float[] s; //previous density
    float[] density;

    float[] Vx; //current x and y
    float[] Vy;

    float[] Vx0; //previous x and y
    float[] Vy0;

    public int IX(int x, int y) => x + y * size;

    //fluid volume initialization
    public FluidVolume(int size, int diffusion, int viscosity, float dt) {
        this.size = size;
        this.dt = dt;
        this.diff = diffusion;
        this.visc = viscosity;

        this.s = new float[size * size];
        this.density = new float[size * size];

        this.Vx = new float[size * size];
        this.Vy = new float[size * size];

        this.Vx0 = new float[size * size];
        this.Vy0 = new float[size * size];
    }

    public void AddDensity(int x, int y, float amount) {
        int index = IX(x, y);
        this.density[index] += amount;
    }

    public void AddVelocity(int x, int y, float amountX, float amountY) {
        int index = IX(x, y);
        this.Vx[index] += amountX;
        this.Vy[index] += amountY;
    }
}
