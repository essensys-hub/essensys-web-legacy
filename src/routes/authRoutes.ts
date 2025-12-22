import { Router } from 'express';
import { logger } from '@/config/logger';

const router = Router();

// TODO: Implémenter les contrôleurs d'authentification
router.post('/login', (req, res) => {
  res.status(501).json({
    success: false,
    error: 'Endpoint en cours d\'implémentation',
    message: 'L\'authentification sera implémentée dans les prochaines itérations',
  });
});

router.post('/register', (req, res) => {
  res.status(501).json({
    success: false,
    error: 'Endpoint en cours d\'implémentation',
    message: 'L\'inscription sera implémentée dans les prochaines itérations',
  });
});

router.post('/refresh', (req, res) => {
  res.status(501).json({
    success: false,
    error: 'Endpoint en cours d\'implémentation',
    message: 'Le rafraîchissement de token sera implémenté dans les prochaines itérations',
  });
});

router.post('/logout', (req, res) => {
  res.status(501).json({
    success: false,
    error: 'Endpoint en cours d\'implémentation',
    message: 'La déconnexion sera implémentée dans les prochaines itérations',
  });
});

export { router as authRoutes };