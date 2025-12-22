import { Router } from 'express';

const router = Router();

router.get('/profile', (req, res) => {
  res.status(501).json({
    success: false,
    error: 'Endpoint en cours d\'implémentation',
    message: 'La gestion des profils utilisateur sera implémentée dans les prochaines itérations',
  });
});

router.put('/profile', (req, res) => {
  res.status(501).json({
    success: false,
    error: 'Endpoint en cours d\'implémentation',
  });
});

export { router as userRoutes };