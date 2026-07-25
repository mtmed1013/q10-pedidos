import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { providePrimeNG } from 'primeng/config';


import { routes } from './app.routes';
import { provideHttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';

import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

const Q10Theme = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#fff7ed',
      100: '#ffedd5',
      200: '#fed7aa',
      300: '#fdba74',
      400: '#fb923c',
      500: '#f97316',
      600: '#ea580c',
      700: '#c2410c',
      800: '#9a3412',
      900: '#7c2d12',
      950: '#431407'
    }
  }
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
    MessageService,
    providePrimeNG({
      license:
        'eyJpZCI6ImRiMmFlMDVkLWJjYTUtNDY5Ny1iZWZkLTJmZWJhMGY1NmUwYiIsInByb2R1Y3QiOiJwcmltZXVpIiwidGllciI6ImNvbW11bml0eSIsInR5cGUiOiJkZXYiLCJpYXQiOjE3ODUwMTMwMzMsImV4cCI6MTgxNjU0OTAzM30.X5Bw804mrSiR87GLLlnkHgJrGK-bQYEeOWp1cMxNstH-ND-PbVQnga1Dy3xkxiKvpV-YAS3yldwz-XNOEPZsAg',
      theme: {
        preset: Q10Theme,
        options: {
          darkModeSelector: false,
        },
      },
    }),
  ],
};
