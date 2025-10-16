# Deployment Guide for Products Management API

This guide will help you deploy your Products Management API to Render with NeonDB.

## Prerequisites

1. A GitHub repository with your code
2. A Render account
3. A NeonDB account (or use Render's managed PostgreSQL)

## Step 1: Set up NeonDB

1. Go to [NeonDB Console](https://console.neon.tech/)
2. Create a new project
3. Note down your connection string (it will look like: `postgres://username:password@host:port/database`)

## Step 2: Deploy to Render

### Option A: Using Render Dashboard

1. Go to [Render Dashboard](https://dashboard.render.com/)
2. Click "New +" → "Web Service"
3. Connect your GitHub repository
4. Configure the service:
   - **Name**: `products-management-api`
   - **Environment**: `Dotnet`
   - **Build Command**: `dotnet publish -c Release -o out`
   - **Start Command**: `dotnet out/Products_Management.dll`
   - **Instance Type**: Free

5. Add Environment Variables:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   DATABASE_URL=postgres://username:password@host:port/database
   CLOUDINARY_CLOUD_NAME=your_cloudinary_cloud_name
   CLOUDINARY_API_KEY=your_cloudinary_api_key
   CLOUDINARY_API_SECRET=your_cloudinary_api_secret
   JWT_SECRET=your_jwt_secret_key
   STRIPE_PUBLISHABLE_KEY=your_stripe_publishable_key
   STRIPE_SECRET_KEY=your_stripe_secret_key
   STRIPE_WEBHOOK_SECRET=your_stripe_webhook_secret
   ```

### Option B: Using render.yaml (Infrastructure as Code)

1. Push your code to GitHub
2. In Render Dashboard, click "New +" → "Blueprint"
3. Connect your repository
4. Render will automatically detect the `render.yaml` file and create the service

## Step 3: Database Migration

After deployment, you need to run database migrations:

1. Go to your service in Render Dashboard
2. Open the Shell/Console
3. Run the migration script:
   ```bash
   ./scripts/migrate-db.sh
   ```

Or manually run:
```bash
dotnet ef database update
```

## Step 4: Verify Deployment

1. Your API will be available at: `https://your-service-name.onrender.com`
2. Swagger documentation: `https://your-service-name.onrender.com/swagger`
3. Test the endpoints to ensure everything works

## Environment Variables Explained

- `DATABASE_URL`: Your NeonDB connection string
- `CLOUDINARY_*`: For image upload functionality
- `JWT_SECRET`: Secret key for JWT token generation
- `STRIPE_*`: For payment processing
- `ASPNETCORE_ENVIRONMENT`: Set to "Production" for production deployment

## Troubleshooting

### Common Issues:

1. **Database Connection Failed**: 
   - Check if `DATABASE_URL` is correctly set
   - Ensure NeonDB allows connections from Render's IP ranges

2. **Migration Failed**:
   - Make sure the database exists
   - Check if the connection string is correct
   - Run migrations manually in the Render shell

3. **Build Failed**:
   - Check if all dependencies are in the `.csproj` file
   - Ensure the build command is correct

4. **Runtime Errors**:
   - Check the logs in Render Dashboard
   - Verify all environment variables are set
   - Ensure the application starts correctly

## Security Notes

- Never commit sensitive information to your repository
- Use Render's environment variables for secrets
- Regularly rotate your JWT secrets and API keys
- Enable SSL/TLS for your database connections

## Monitoring

- Monitor your application logs in Render Dashboard
- Set up alerts for errors
- Monitor database performance in NeonDB console
- Track API usage and performance

## Scaling

- Upgrade to a paid plan for better performance
- Use Render's auto-scaling features
- Consider database connection pooling
- Monitor resource usage

## Support

- Render Documentation: https://render.com/docs
- NeonDB Documentation: https://neon.tech/docs
- .NET Core Documentation: https://docs.microsoft.com/en-us/aspnet/core/
