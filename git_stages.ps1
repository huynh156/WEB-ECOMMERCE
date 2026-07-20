$git = "C:\Program Files\Git\cmd\git.exe"

# Initialize repo if not initialized
if (!(Test-Path -Path ".git")) {
    & $git init
}

# Set remote origin
& $git remote remove origin 2>$null
& $git remote add origin https://github.com/huynh156/WEB-ECOMMERCE.git

# Set local user config so commit succeeds
& $git config user.name "Huynh"
& $git config user.email "huynh156@example.com"

# Stage 1: Init project template
Write-Host "Creating Commit 1..."
& $git add FashionHubWeb.csproj Properties/launchSettings.json
& $git commit -m "feat: init project template structure"

# Stage 2: Database entities & Context
Write-Host "Creating Commit 2..."
& $git add Models/Brand.cs Models/Category.cs Models/Product.cs Models/User.cs Models/Cart.cs Models/Order.cs Models/OrderDetail.cs Models/FashionHubContext.cs Models/CategoryPromotion.cs Models/ProductPromotion.cs Models/Review.cs Models/Wishlist.cs Models/Shipper.cs Models/Status.cs Models/Coupon.cs Models/PurchaseOrderDetail.cs Models/PurchaseOrder.cs Models/Supplier.cs
& $git commit -m "feat: setup database context and entities mapping"

# Stage 3: Initial seeder
Write-Host "Creating Commit 3..."
& $git add Models/DataSeeder.cs
& $git commit -m "feat: implement database data seeder"

# Stage 4: Admin Panel Areas
Write-Host "Creating Commit 4..."
& $git add Areas/Admin/
& $git commit -m "feat: build Admin Panel Areas for products and order management"

# Stage 5: User Shop pages
Write-Host "Creating Commit 5..."
& $git add Controllers/ShopController.cs Views/Shop/
& $git commit -m "feat: build user shop page and category/brand filters"

# Stage 6: Basic Cart & Wishlist views
Write-Host "Creating Commit 6..."
& $git add Views/Cart/Index.cshtml Views/Wishlist/
& $git commit -m "feat: build basic cart and wishlist view pages"

# Stage 7: Auth & Google Login
Write-Host "Creating Commit 7..."
& $git add Controllers/AccountController.cs Views/Account/Login.cshtml Views/Account/Register.cshtml
& $git commit -m "feat: integrate Google OAuth authentication"

# Stage 8: VNPAY Integration Helper
Write-Host "Creating Commit 8..."
& $git add Helper/VnPayLibrary.cs Services/IVnPayService.cs Services/VnPayService.cs Models/VnPaymentResponseModel.cs
& $git commit -m "feat: integrate VNPAY payment gateway library & services"

# Stage 9: PayPal Integration Helper
Write-Host "Creating Commit 9..."
& $git add Helper/PaypalClient.cs
& $git commit -m "feat: integrate PayPal REST API payment helper client"

# Stage 10: Checkout form & callbacks
Write-Host "Creating Commit 10..."
& $git add Views/Cart/CheckOut.cshtml Views/Cart/PaymentSuccess.cshtml Views/Cart/PaymentFail.cshtml Models/CheckOutVM.cs
& $git commit -m "feat: build checkout form view & handle payment gateway callbacks"

# Stage 11: Mini-Cart Drawer
Write-Host "Creating Commit 11..."
& $git add Views/Cart/_MiniCartPartial.cshtml
& $git commit -m "feat: implement sliding mini-cart offcanvas drawer"

# Stage 12: Wishlist Toggle API
Write-Host "Creating Commit 12..."
& $git add Controllers/WishlistController.cs
& $git commit -m "feat: support AJAX wishlist toggle endpoint"

# Stage 13: Customer Order History
Write-Host "Creating Commit 13..."
& $git add Views/Account/Orders.cshtml
& $git commit -m "feat: build customer order history dashboard"

# Stage 14: Final Integration & Styles
Write-Host "Creating Commit 14..."
& $git add .
& $git commit -m "feat: optimize premium UI/UX theme, add hover zoom, coupons & related products"

Write-Host "All commits created successfully!"
