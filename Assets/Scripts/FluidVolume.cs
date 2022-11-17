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

    /// <summary>
    /// Simulation Step
    /// </summary>
    public void Step() {
        float visc = this.visc;
        float diff = this.diff;
        float dt = this.dt;
        float[] Vx = this.Vx;
        float[] Vy = this.Vy;
        float[] Vx0 = this.Vx0;
        float[] Vy0 = this.Vy0;
        float[] s = this.s;
        float[] density = this.density;

        Diffuse(1, Vx0, Vx, visc, dt);
        Diffuse(2, Vy0, Vy, visc, dt);

        Project(Vx0, Vy0, Vx, Vy);

        Advect(1, Vx, Vx0, Vx0, Vy0, dt);
        Advect(2, Vy, Vy0, Vx0, Vy0, dt);

        Project(Vx, Vy, Vx0, Vy0);

        Diffuse(0, s, density, diff, dt);
        Advect(0, density, s, Vx, Vy, dt);
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

    #region Simulation Steps
    /* 1. diffuse
     * 2. project
     * 3. advect
     */

    /// <summary>
    /// Diffuses the Liquid by spreading diffusion and velocity values
    /// </summary>
    void Diffuse(int b, float[] x, float[] x0, float diff, float dt) {
        float a = dt * diff * (size - 2) * (size - 2);
        lin_solve(b, x, x0, a, 1 + 6 * a);
    }

    /// <summary>
    /// Fixes net in and outflow in cells and restores equilibrium
    /// </summary>
    void Project(float[] velocX, float[] velocY, float[] p, float[] div) {
        for (int j = 1; j < size - 1; j++)
        {
            for (int i = 1; i < size - 1; i++)
            {
                div[IX(i, j)] = -0.5f * (
                            velocX[IX(i + 1, j)]
                        - velocX[IX(i - 1, j)]
                        + velocY[IX(i, j + 1)]
                        - velocY[IX(i, j - 1)]
                    ) / size;
                p[IX(i, j)] = 0;
            }
        }
        set_bnd(0, div);
        set_bnd(0, p);
        lin_solve(0, p, div, 1, 6);

        for (int j = 1; j < size - 1; j++)
        {
            for (int i = 1; i < size - 1; i++)
            {
                velocX[IX(i, j)] -= 0.5f * (p[IX(i + 1, j)] - p[IX(i - 1, j)]) * size;
                velocY[IX(i, j)] -= 0.5f * (p[IX(i, j + 1)] - p[IX(i, j - 1)]) * size;
            }
        }
        set_bnd(1, velocX);
        set_bnd(2, velocY);
    }

    /// <summary>
    /// Moves Liquid according to its velocities
    /// </summary>
    void Advect(int b, float[] d, float[] d0, float[] velocX, float[] velocY, float dt)
    {
        float i0, i1, j0, j1;

        float dtx = dt * (size - 2);
        float dty = dt * (size - 2);

        float s0, s1, t0, t1;
        float tmp1, tmp2, x, y;

        float Nfloat = size;
        float ifloat, jfloat;
        int i, j;

        for (j = 1, jfloat = 1; j < size - 1; j++, jfloat++)
        {
            for (i = 1, ifloat = 1; i < size - 1; i++, ifloat++)
            {
                tmp1 = dtx * velocX[IX(i, j)];
                tmp2 = dty * velocY[IX(i, j)];
                x = ifloat - tmp1;
                y = jfloat - tmp2;

                if (x < 0.5f) x = 0.5f;
                if (x > Nfloat + 0.5f) x = Nfloat + 0.5f;
                i0 = Mathf.Floor(x);
                i1 = i0 + 1.0f;
                if (y < 0.5f) y = 0.5f;
                if (y > Nfloat + 0.5f) y = Nfloat + 0.5f;
                j0 = Mathf.Floor(y);
                j1 = j0 + 1.0f;

                s1 = x - i0;
                s0 = 1.0f - s1;
                t1 = y - j0;
                t0 = 1.0f - t1;

                int i0i = (int)i0;
                int i1i = (int)i1;
                int j0i = (int)j0;
                int j1i = (int)j1;

                d[IX(i, j)] =
                     s0 * (t0 * d0[IX(i0i, j0i)] + t1 * d0[IX(i0i, j1i)])
                   + s1 * (t0 * d0[IX(i1i, j0i)] + t1 * d0[IX(i1i, j1i)]);
            }
        }
        set_bnd(b, d);
    }
    #endregion

    /// <summary>
    /// Solving a linear differential equation
    /// </summary>
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

    /// <summary>
    /// Set bounds. Reflect Liquid inertia along the borders of the Liquid Volume
    /// </summary>
    void set_bnd(int b, float[] x)
    {
        //check for borders bottom and top
        for (int i = 1; i < size - 1; i++)
        {
            x[IX(i, 0)] = b == 2 ? -x[IX(i, 1)] : x[IX(i, 1)];
            x[IX(i, size - 1)] = b == 2 ? -x[IX(i, size - 2)] : x[IX(i, size - 2)];
        }

        //check for borders left and right
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
