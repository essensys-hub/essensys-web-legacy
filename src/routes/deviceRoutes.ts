import { Router } from 'express';

const router = Router();

router.get('/', (req, res) => {
  res.status(501).json({
    success: false,
    error: 'Endpoint en cours d\'implémentation',
    message: 'La gestion des appareils sera implémentée dans les prochaines itérations',
  });
});

export { router as deviceRoutes };