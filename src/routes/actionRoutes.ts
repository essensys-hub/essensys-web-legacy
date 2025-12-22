import { Router } from 'express';

const router = Router();

router.post('/', (req, res) => {
  res.status(501).json({
    success: false,
    error: 'Endpoint en cours d\'implémentation',
    message: 'La gestion des actions sera implémentée dans les prochaines itérations',
  });
});

router.get('/', (req, res) => {
  res.status(501).json({
    success: false,
    error: 'Endpoint en cours d\'implémentation',
  });
});

export { router as actionRoutes };