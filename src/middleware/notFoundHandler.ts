import { Request, Response, NextFunction } from 'express';

export const notFoundHandler = (req: Request, res: Response, next: NextFunction) => {
  res.status(404).json({
    success: false,
    error: 'Endpoint non trouvé',
    message: `La route ${req.method} ${req.path} n'existe pas`,
    availableEndpoints: [
      'GET /api/health',
      'POST /api/auth/login',
      'POST /api/auth/register',
      'GET /api/users/profile',
      'GET /api/machines',
      'GET /api/devices',
      'POST /api/actions',
    ],
  });
};