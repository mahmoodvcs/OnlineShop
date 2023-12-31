﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MahtaKala.Entities.EntityConfig
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // This Converter will perform the conversion to and from Json to the desired type
            builder.Property(e => e.ImageList).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<IList<string>>(v, (JsonSerializerOptions)null));

            builder.Property(e => e.Characteristics).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<IList<Characteristic>>(v, (JsonSerializerOptions)null));

            builder.Property(e => e.Properties).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<IList<KeyValuePair<string, string>>>(v, (JsonSerializerOptions)null));

            builder.HasOne(a => a.Seller).WithMany(a => a.Products).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(a => a.Brand).WithMany(a => a.Products).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(a => a.Supplier).WithMany(a => a.Products).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
