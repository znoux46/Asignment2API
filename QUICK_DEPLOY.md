# Quick Deploy Guide - Products Management API to Render + NeonDB

## 🚀 Ready to Deploy!

Your project is now configured for deployment to Render with NeonDB. Here's what I've set up for you:

### ✅ Files Created/Modified:

1. **`appsettings.Production.json`** - Production configuration with environment variables
2. **`render.yaml`** - Infrastructure as Code configuration for Render
3. **`Dockerfile.production`** - Optimized Docker configuration
4. **`Program.cs`** - Updated with NeonDB connection handling and auto-migration
5. **`scripts/migrate-db.sh`** - Database migration script
6. **`deploy.ps1`** - Local deployment preparation script
7. **`DEPLOYMENT.md`** - Detailed deployment guide

## 🎯 Next Steps:

### 1. Set up NeonDB
- Go to [NeonDB Console](https://console.neon.tech/)
- Create a new project
- Copy your connection string (format: `postgres://username:password@host:port/database`)

### 2. Deploy to Render
**Option A: Quick Deploy (Recommended)**
1. Push your code to GitHub
2. Go to [Render Dashboard](https://dashboard.render.com/)
3. Click "New +" → "Blueprint"
4. Connect your GitHub repository
5. Render will auto-detect `render.yaml` and create your service

**Option B: Manual Deploy**
1. Go to [Render Dashboard](https://dashboard.render.com/)
2. Click "New +" → "Web Service"
3. Connect GitHub repository
4. Use these settings:
   - **Build Command**: `dotnet publish -c Release -o out`
   - **Start Command**: `dotnet out/Products_Management.dll`
   - **Environment**: `Dotnet`

### 3. Set Environment Variables in Render
Add these environment variables in your Render service settings:

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

### 4. Deploy!
- Click "Deploy" in Render
- Wait for the build to complete
- Your API will be available at: `https://your-service-name.onrender.com`
- Swagger docs: `https://your-service-name.onrender.com/swagger`

## 🔧 Key Features Added:

- ✅ **NeonDB Integration**: Automatic connection string parsing
- ✅ **Auto-Migration**: Database migrations run automatically on startup
- ✅ **Production Ready**: Optimized for Render deployment
- ✅ **Environment Variables**: Secure configuration management
- ✅ **Docker Support**: Containerized deployment option
- ✅ **CORS Configured**: Ready for frontend integration

## 🐛 Troubleshooting:

If you encounter issues:
1. Check Render logs for error messages
2. Verify all environment variables are set
3. Ensure NeonDB allows connections from Render
4. Check the detailed guide in `DEPLOYMENT.md`

## 📞 Support:

- Render Docs: https://render.com/docs
- NeonDB Docs: https://neon.tech/docs
- Your API will be live at: `https://your-service-name.onrender.com`

---

**🎉 You're all set! Your Products Management API is ready for production deployment.**
