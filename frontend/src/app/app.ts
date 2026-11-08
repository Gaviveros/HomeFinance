import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [FormsModule],
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {
  email = '';
  password = '';
  householdName = '';
  accessToken = '';
  currentUserId = '';
  message = 'Listo para crear tu primer hogar.';
  loading = false;
  apiUrl = 'https://localhost:7265/api';

  constructor(private readonly http: HttpClient) {}

  async register(): Promise<void> {
    this.loading = true;
    this.message = 'Creando usuario...';

    this.http.post<{ message: string; userId: string; email: string }>(`${this.apiUrl}/auth/register`, {
      email: this.email,
      password: this.password,
    }).subscribe({
      next: (response) => {
        this.message = `Usuario creado: ${response.email}`;
        this.loading = false;
      },
      error: (error) => {
        this.message = error?.error?.message ?? 'No se pudo completar el registro.';
        this.loading = false;
      },
    });
  }

  async login(): Promise<void> {
    this.loading = true;
    this.message = 'Validando credenciales...';

    this.http.post<{
      message: string;
      userId: string;
      email: string;
      accessToken: string;
      expiresAt: string;
    }>(`${this.apiUrl}/auth/login`, {
      email: this.email,
      password: this.password,
    }).subscribe({
      next: (response) => {
        this.accessToken = response.accessToken;
        this.currentUserId = response.userId;
        this.message = `Sesión iniciada para ${response.email}`;
        this.loading = false;
      },
      error: (error) => {
        this.message = error?.error?.message ?? 'Credenciales inválidas.';
        this.loading = false;
      },
    });
  }

  createHousehold(): void {
    if (!this.accessToken) {
      this.message = 'Primero inicia sesión para crear un hogar.';
      return;
    }

    this.loading = true;
    this.message = 'Creando hogar...';

    this.http.post<{ message: string; household: { id: string; name: string; currency: string; timeZone: string } }>(
      `${this.apiUrl}/households`,
      { name: this.householdName },
      { headers: { Authorization: `Bearer ${this.accessToken}` } }
    ).subscribe({
      next: (response) => {
        this.message = `${response.message} - ${response.household.name}`;
        this.loading = false;
      },
      error: (error) => {
        this.message = error?.error?.message ?? 'No se pudo crear el hogar.';
        this.loading = false;
      },
    });
  }
}
