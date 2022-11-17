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

    /// <summary>
    /// Convert int vector to index x + y * size
    /// </summary>
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

    // Linear Solve
    void lin_solve(int b, float[] x, float[] x0, float a, float c)
    {
        float cRecip = 1.0f / c;
        for (int j = 1; j < size - 1; j++)
        {
            for (int i = 1; i < size - 1; i++)
            {
                x[IX(i, j)] =
                    (x0[IX(i, j)]
                        + a * (     x[IX(i + 1, j)]
                                    + x[IX(i - 1, j)]
                                    + x[IX(i, j + 1)]
                                    + x[IX(i, j - 1)]
                        )) * cRecip;
            }
        }
        set_bnd(b, x);
    }

    void set_bnd(int b, float[] x)
    {
        for (int i = 1; i < size - 1; i++)
        {
            x[IX(i, 0)] = b == 2 ? -x[IX(i, 1)] : x[IX(i, 1)];
            x[IX(i, size - 1)] = b == 2 ? -x[IX(i, size - 2)] : x[IX(i, size - 2)];
        }

        for (int j = 1; j < size - 1; j++)
        {
            x[IX(0, j)] = b == 1 ? -x[IX(1, j)] : x[IX(1, j)];
            x[IX(size - 1, j)] = b == 1 ? -x[IX(size - 2, j)] : x[IX(size - 2, j)];
        }

        x[IX(0, 0)]                 = 0.5f * (x[IX(1, 0)] + x[IX(0, 1)]);
        x[IX(0, size - 1)]          = 0.5f * (x[IX(1, size - 1)] + x[IX(0, size - 2)]);
        x[IX(size - 1, 0)]          = 0.5f * (x[IX(size - 2, 0)] + x[IX(size - 1, 1)]);
        x[IX(size - 1, size - 1)]   = 0.5f * (x[IX(size - 2, size - 1)] + x[IX(size - 1, size - 2)]);
    }
}
